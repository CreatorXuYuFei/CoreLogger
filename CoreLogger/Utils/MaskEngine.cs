using CoreLogger.Entiy;
using System.Text.RegularExpressions;

namespace CoreLogger.Utils
{
    internal static class MaskEngine
    {
        ///<summary>
        ///正则分组脱敏，手机号/身份证生效
        ///</summary>
        public static string Mask(string input, SensitiveMaskOptions options)
        {
            if (!options.Enabled || string.IsNullOrEmpty(input)) return input;
            foreach (var rule in options.Rules)
            {
                //正确使用分组替换，兼容原配置规则
                input = Regex.Replace(input, rule.Key, m =>
                {
                    var value = m.Value;
                    //手机号：1开头11位 → 1****xxxx
                    if (value.Length == 11 && value.StartsWith('1'))
                        return $"{value[0]}****{value[7..]}";
                    //身份证18位 → 6*************4
                    if (value.Length == 18)
                        return $"{value[..6]}****************{value[16..]}";
                    //身份证15位（兼容）
                    if (value.Length == 15)
                        return $"{value[..6]}***********{value[12..]}";
                    return value;
                });
            }
            return input;
        }
    }
}
