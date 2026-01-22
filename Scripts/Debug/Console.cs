using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
// using UnityEngine.UI;

// 이 스크립트는 콘솔을 생성하고 명령어를 실행하는 기능을 제공합니다.
public class Console : MonoBehaviour
{
    public TMP_InputField CommandField;
    public TMP_Text OutputText;
    private Dictionary<string, Tuple<Func<string[], string>, string>> _commands = new Dictionary<string, Tuple<Func<string[], string>, string>>();
    private string _currentInput;
    private bool _canChangeInput = true;
    private List<string> _matchingCommands = new List<string>();
    private int _autoCompleteIndex = 0;

    private void Start()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        CommandField = transform.GetChild(0).GetChild(0).GetComponent<TMP_InputField>();
        OutputText = transform.GetChild(0).GetChild(1).GetComponent<TMP_Text>();

        // 명령어 추가
        _commands.Add("help", new Tuple<Func<string[], string>, string>(Help, "Show Commands Description"));
        _commands.Add("setWallInstantly", CreateCommand((Action<Vector2Int, bool>)GameManager.Instance.CharacterController.SetWallInstantly, "Wall set at {0}, IsHorizontal: {1}\n", "setWall ({x}, {y}) {isHorizontal}"));
        _commands.Add("setPreviewWall", CreateCommand((Action<Vector2Int, bool>)GameManager.Instance.CharacterController.SetWallPreview, "Preview set at {0}, IsHorizontal: {1}\n", "setPreview ({x}, {y}) {isHorizontal}"));
        _commands.Add("setWall", CreateCommand((Action)GameManager.Instance.CharacterController.SetWall, "Wall set\n", "setWall"));

        // 이벤트 추가
        CommandField.onEndEdit.AddListener(delegate { ExecuteCommand(CommandField.text); CommandField.ActivateInputField(); });
        CommandField.onValueChanged.AddListener(delegate { OnInputChanged(); });
    }
    private void Update()
    {
        // 콘솔 열기/닫기 ( ` 키로 상호작용)
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            bool currentActive = transform.GetChild(0).gameObject.activeSelf;
            transform.GetChild(0).gameObject.SetActive(!currentActive);
            // 콘솔 활성화에 따라 로그 이벤트 추가/제거
            if (currentActive) Application.logMessageReceived -= HandleLog;
            else
            {
                Application.logMessageReceived += HandleLog;
                CommandField.ActivateInputField();
            }
        }
        // 자동완성 (Tab 키로 상호작용)
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            AutoCompleteCommand();
        }
    }
    private void OnInputChanged()
    {
        if (!_canChangeInput)
            return;
        _currentInput = CommandField.text;
        _matchingCommands.Clear();
        _autoCompleteIndex = 0;
    }
    // 로그 이벤트 핸들러
    private void HandleLog(string logString, string stackTrace, LogType type)
    {
        string trimmedLog = logString.Trim();
        // 로그를 콘솔에 추가
        StartCoroutine(UpdateTextMesh(trimmedLog));
    }
    // UI 갱신을 위한 코루틴 (코루틴 미사용 시 log가 정상적으로 출력되지 않음)
    private IEnumerator UpdateTextMesh(string log)
    {
        yield return null; // 한 프레임 대기
        OutputText.text += $"{log}\n";
    }
    // 명령어 생성 (특정 함수들을 콘솔에 맞게 변환(log, description 추가))
    private Tuple<Func<string[], string>, string> CreateCommand(Delegate func, string log, string description)
    {
        return new Tuple<Func<string[], string>, string>((string[] args) =>
        {
            // func의 매개변수 타입을 가져오기
            var methodParams = func.Method.GetParameters();
            if (args.Length != methodParams.Length)
            {
                return "Invalid number of arguments";
            }
            object[] convertedArgs = new object[methodParams.Length];

            for (int i = 0; i < methodParams.Length; i++)
            {
                Type paramType = methodParams[i].ParameterType;

                try
                {
                    if (paramType == typeof(Vector2Int))
                    {
                        convertedArgs[i] = ParseVector2Int(args[i]);
                    }
                    else
                    {
                        // TypeConverter로 변환 시도
                        TypeConverter converter = TypeDescriptor.GetConverter(paramType);
                        convertedArgs[i] = converter.ConvertFromString(args[i]);
                    }
                }
                catch (Exception ex)
                {
                    throw new ArgumentException($"Error converting argument {i} to {paramType}: {ex.Message}");
                }
            }
            string output = "";
            try
            {
                output += func.DynamicInvoke(convertedArgs)?.ToString();
                output += "\n";
                string.Format(log, convertedArgs);
            }
            catch (Exception ex)
            {
                output += ex.Message;
                output += "\n";
            }

            return output;
        }, description);
    }
    // Help 명령어 (가능 명령어 및 명령어의 설명 출력)
    string Help(string[] args)
    {
        string output = "";
        if (args.Length == 0)
        {
            output += "Available commands:\n";
            foreach (var command in _commands.Keys)
            {
                output += $"{command}: {_commands[command].Item2}" + "\n";
            }
            return output;
        }
        if (_commands.ContainsKey(args[0]))
        {
            return $"{args[0]}: {_commands[args[0]].Item2}";
        }
        return "Command not found";
    }
    // Parse 관련 툴
    //! 추후 Utils로 이동 가능성 있음
    private Vector2Int ParseVector2Int(string input)
    {
        input = input.Trim('(', ')'); // 괄호 제거
        var parts = input.Split(',');

        if (parts.Length != 2)
            throw new FormatException("Input string is not in the correct format for Vector2 (e.g., '(x, y)').");

        int x = int.Parse(parts[0]);
        int y = int.Parse(parts[1]);

        return new Vector2Int(x, y);
    }
    private string[] ParseArguments(string input)
    {
        // 정규식: 괄호로 묶인 "(x, y)"를 찾거나, 공백 기준으로 나눕니다.
        var matches = Regex.Matches(input, @"\([^)]+\)|\S+");
        List<string> args = new List<string>();

        foreach (Match match in matches)
        {
            args.Add(match.Value);
        }

        return args.ToArray();
    }

    // 자동완성
    private void AutoCompleteCommand()
    {
        if (string.IsNullOrWhiteSpace(_currentInput))
            return;

        // 현재 입력으로 시작하는 명령어 검색 (첫 Tab 누를 때만)
        if (_matchingCommands.Count == 0)
        {
            _matchingCommands = _commands.Keys.Where(cmd => cmd.StartsWith(_currentInput, StringComparison.OrdinalIgnoreCase)).ToList();

            if (_matchingCommands.Count == 0)
            {
                // OutputText.text += $"No matching commands found for '{_currentInput}'\n";
                return;
            }
        }

        _canChangeInput = false;
        // 자동완성 순환
        CommandField.text = _matchingCommands[_autoCompleteIndex];
        CommandField.caretPosition = CommandField.text.Length; // 커서 위치 끝으로 이동

        _canChangeInput = true;
        // 다음 인덱스 준비
        _autoCompleteIndex = (_autoCompleteIndex + 1) % _matchingCommands.Count;

    }
    // 명령어 실행
    private void ExecuteCommand(string command)
    {
        if (string.IsNullOrWhiteSpace(command))
            return;

        // 명령어와 인수 분리
        string[] splitCommand = ParseArguments(command);
        string commandKey = splitCommand[0];
        string[] args = new string[splitCommand.Length - 1];
        System.Array.Copy(splitCommand, 1, args, 0, args.Length);


        // 명령어 실행
        if (_commands.ContainsKey(commandKey))
        {
            OutputText.text += _commands[commandKey]?.Item1.Invoke(args);
        }
        else
        {
            OutputText.text += $"Unknown command: {command}\n";
        }

        CommandField.text = "";
    }
}