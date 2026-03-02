/// <summary>
/// 文本工具类
/// 提供字符串处理的扩展方法，包括空白字符检测、去除空白字符和特殊字符
/// 使用高效的内存操作优化性能，避免创建大量临时字符串
/// </summary>
public static class TextUtility
{
    /// <summary>
    /// 检查字符是否为空白字符（扩展方法）
    /// 支持Unicode中各种空白字符，包括空格、制表符、换行符、全角空格等
    /// 使用Switch-case进行高效匹配，支持多种空白字符类型
    /// </summary>
    /// <param name="character">要检查的字符</param>
    /// <returns>如果是空白字符返回true，否则返回false</returns>
    public static bool IsWhitespace(this char character)
    {
        switch (character)
        {
            // 普通空格和不同语言的空格
            case '\u0020':  // 普通空格
            case '\u00A0':  // 不换行空格
            case '\u1680':  // 欧甘文空格
            case '\u2000':  // 半身em空格
            case '\u2001':  // 全身em空格
            case '\u2002':  // 半身en空格
            case '\u2003':  // 全身en空格
            case '\u2004':  // 三分之一em空格
            case '\u2005':  // 四分之一em空格
            case '\u2006':  // 六分之一em空格
            case '\u2007':  // 数字空格
            case '\u2008':  // 标点空格
            case '\u2009':  // 窄空格
            case '\u200A':  // 细空格
            case '\u202F':  // 窄不换行空格
            case '\u205F':  // 中数学空格
            case '\u3000':  // 全角空格
            // 行分隔符和段落分隔符
            case '\u2028':  // 行分隔符
            case '\u2029':  // 段落分隔符
            // 控制字符：制表符、换行、垂直制表符、换页、回车
            case '\u0009':  // 水平制表符
            case '\u000A':  // 换行
            case '\u000B':  // 垂直制表符
            case '\u000C':  // 换页
            case '\u000D':  // 回车
            case '\u0085':  // 下一行
            {
                return true;
            }

            default:
            {
                return false;
            }
        }
    }

    /// <summary>
    /// 去除字符串中的所有空白字符（扩展方法）
    /// 高效实现：通过字符数组原地操作，避免创建多个中间字符串
    /// 参考实现：https://stackoverflow.com/a/37368176
    /// 性能对比：https://stackoverflow.com/a/37347881
    /// </summary>
    /// <param name="text">输入字符串</param>
    /// <returns>移除所有空白字符后的新字符串</returns>
    public static string RemoveWhitespaces(this string text)
    {
        int textLength = text.Length;
        char[] textCharacters = text.ToCharArray();  // 转换为字符数组进行原地操作
        int currentWhitespacelessTextLength = 0;  // 记录无空白字符的当前长度

        for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
        {
            char currentTextCharacter = textCharacters[currentCharacterIndex];

            if (currentTextCharacter.IsWhitespace())
            {
                continue;  // 跳过空白字符
            }

            // 将非空白字符前移
            textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
        }

        // 创建新字符串，只包含前currentWhitespacelessTextLength个字符
        return new string(textCharacters, 0, currentWhitespacelessTextLength);
    }

    /// <summary>
    /// 去除字符串中的特殊字符（扩展方法）
    /// 只保留字母、数字和空白字符，移除其他所有特殊字符
    /// 与RemoveWhitespaces类似，但保留空白字符
    /// 参考实现：https://stackoverflow.com/questions/3210393/how-do-i-remove-all-non-alphanumeric-characters-from-a-string-except-dash
    /// </summary>
    /// <param name="text">输入字符串</param>
    /// <returns>移除特殊字符后的新字符串</returns>
    public static string RemoveSpecialCharacters(this string text)
    {
        int textLength = text.Length;
        char[] textCharacters = text.ToCharArray();
        int currentWhitespacelessTextLength = 0;

        for (int currentCharacterIndex = 0; currentCharacterIndex < textLength; ++currentCharacterIndex)
        {
            char currentTextCharacter = textCharacters[currentCharacterIndex];

            // 如果不是字母或数字，并且不是空白字符，则跳过
            if (!char.IsLetterOrDigit(currentTextCharacter) && !currentTextCharacter.IsWhitespace())
            {
                continue;
            }

            // 保留字母、数字和空白字符
            textCharacters[currentWhitespacelessTextLength++] = currentTextCharacter;
        }

        return new string(textCharacters, 0, currentWhitespacelessTextLength);
    }
}