using System.Collections.Generic;

/// <summary>
/// 유물 전용 수식 평가기
/// 지원 연산: *, +, -, /
/// 지원 피연산자: 숫자 리터럴, GlobalSource 참조
/// 좌→우 순차 평가 (연산자 우선순위 없음)
/// </summary>
public static class RelicExpressionEvaluator
{
    /// <summary>
    /// baseValue에 Expression을 적용한 결과를 반환
    /// </summary>
    /// <param name="baseValue">GlobalSource에서 해석된 기본값</param>
    /// <param name="expression">수식 문자열 (예: "* 2", "* SynergyCount_탱커 * 3")</param>
    public static float Evaluate(float baseValue, string expression)
    {
        if (string.IsNullOrWhiteSpace(expression))
            return baseValue;

        float result = baseValue;
        var tokens = TokenizeExpression(expression);

        for (int i = 0; i < tokens.Count; i += 2)
        {
            // tokens[i] = 연산자, tokens[i+1] = 피연산자
            if (i + 1 >= tokens.Count) break;

            string op = tokens[i];
            float operand = ResolveOperand(tokens[i + 1]);

            result = op switch
            {
                "*" => result * operand,
                "+" => result + operand,
                "-" => result - operand,
                "/" => operand != 0 ? result / operand : result,
                _ => result
            };
        }

        return result;
    }

    /// <summary>
    /// 수식 문자열을 [연산자, 피연산자, 연산자, 피연산자, ...] 토큰 배열로 분할
    /// 예: "* SynergyCount_탱커 * 3" -> ["*", "SynergyCount_탱커", "*", "3"]
    /// </summary>
    private static List<string> TokenizeExpression(string expression)
    {
        var tokens = new List<string>();
        var parts = expression.Trim().Split(' ', System.StringSplitOptions.RemoveEmptyEntries);
        tokens.AddRange(parts);
        return tokens;
    }

    /// <summary>
    /// 피연산자를 float으로 해석
    /// 숫자 리터럴이면 직접 변환, 아니면 GlobalDataProvider에서 조회
    /// </summary>
    private static float ResolveOperand(string operand)
    {
        if (float.TryParse(operand, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float numericValue))
            return numericValue;

        // GlobalSource 참조
        return GlobalDataProvider.Resolve(operand);
    }
}
