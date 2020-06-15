using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtfToTxt
{
    public static class rtf2txt
    {
        /// <summary>
        /// 某些特殊符号
        /// </summary>
        static Dictionary<string, string> _dic_rtf2txt = new Dictionary<string, string>() {  {"par", "\n" },
            { "sect","\n" },
            { "page", "\xc"},{
      "line", "\n"},{
      "tab", "\t"},{
      "emdash", "\u2014"},{
      "endash", "\u2013"},{
      "emspace", "\u2003"},{
      "enspace", "\u2002"},{
      "qmspace", "\u2005"},{
      "bullet", "\u2022"},{
      "lquote", "\u2018"},{
      "rquote", "\u2019"},{
      "ldblquote", "\u201C"},{
      "rdblquote", "\u201D" } };
        /// <summary>
        /// 识别RTF格式内容中的文本
        /// </summary>
        /// <param name="rtf">rtf格式文本</param>
        /// <returns>识别到的文本</returns>
        public static string RtfToTxt(string rtf)
        {
            StringBuilder sb = new StringBuilder();
            RtfToTxt(rtf, sb, 0, false, null);
            string rt = sb.ToString();
            //移除最后的换行
            if (rt.EndsWith("\n"))
                rt = rt.Remove(rt.Length - 1);
            return rt;
        }
        /// <summary>
        /// 主要识别函数
        /// </summary>
        /// <param name="rtf">某段rtf格式文本</param>
        /// <param name="sb">用以串联输出结果</param>
        /// <param name="startIndex">当前函数解析的起始位置</param>
        /// <param name="comment">当前内容是否为注释</param>
        /// <param name="globalSettings">全局设置</param>
        /// <returns>本函数解析的字符数</returns>
        static public int RtfToTxt(string rtf, StringBuilder sb, int startIndex = 0, bool comment = false, dynamic[] globalSettings = null)
        {
            if (string.IsNullOrEmpty(rtf)) { return 0; }
            //设置全局项目
            if (globalSettings == null)
                globalSettings = new dynamic[] { Encoding.Default };
            //起始索引
            if (startIndex >= rtf.Length) return 0;
            //本层变量（编码转换数据，一般是多个连续的\'xx）
            List<byte> data = new List<byte>();
            bool first = true;
            //识别内容并添加到sb中
            void add(string content)
            {
                if (string.IsNullOrEmpty(content) || comment)
                {
                    return;
                }
                if (content.Length == 1)
                    goto RET;
                int idx;
                string type;
                if (content[0] != '\\')
                {
                    for (idx = 0; idx < content.Length && content[idx] != ' ' && content[idx] != '\r' && content[idx] != '\n'; idx++) ;
                    if (idx == content.Length) idx = -1;
                    type = (idx < 0 ? content : content.Remove(idx));
                    switch (type)
                    {
                        case "HYPERLINK":
                            if (first)
                                goto RET;
                            goto default;
                        default:
                            sb.Append(content.Replace("\r\n", ""));
                            break;
                    }
                    goto RET;
                }
                string before = "";
                if (content[1] == '\'')
                {
                    data.Add(byte.Parse(content.Substring(2, 2), System.Globalization.NumberStyles.HexNumber));
                    if (content.Length > 4)
                    {
                        before = globalSettings[0].GetString(data.ToArray());
                        data.Clear();
                        sb.Append(before);
                        sb.Append(content.Substring(4));
                    }
                    goto RET;
                }
                if (data.Count != 0)
                {
                    before = globalSettings[0].GetString(data.ToArray());
                    data.Clear();
                }
                else before = "";
                if (content.Length >= 2)
                {
                    switch (content[1])
                    {
                        case '*':
                            if (first)
                                comment = true;
                            goto RET;
                        case '{':
                        case '}':
                        case '\\':
                            sb.Append(before);
                            sb.Append(content, 1, content.Length - 1);
                            goto RET;
                        case 'u':
                            if ((content.Length > 3 && char.IsNumber(content[2])) || (content.Length > 4 && content[2] == '-' && char.IsNumber(content[3])))
                            {
                                int ne = content.IndexOf('?', 3);
                                if (ne > 0)
                                {
                                    sb.Append(before);
                                    sb.Append(((char)int.Parse(content.Substring(2, ne - 2))));
                                    goto RET;
                                }
                            }
                            goto default;
                        default:
                            if (content.Length == 2)
                                sb.Append(before);
                            break;
                    }
                }
                for (idx = 0; idx < content.Length && content[idx] != ' ' && content[idx] != '\r' && content[idx] != '\n'; idx++) ;
                if (idx == content.Length) idx = -1;
                type = (idx < 0 ? content.Substring(1) : content.Substring(1, idx - 1));
                string addstr = null;
                if (_dic_rtf2txt.TryGetValue(type, out addstr))
                {
                    before += addstr;
                }
                else
                    switch (type)
                    {
                        case "ansi":
                            globalSettings[0] = Encoding.Default;
                            break;
                        case "fonttbl"://忽略字符集列表
                        case "colortbl"://忽略色彩表
                            comment = true;
                            break;
                        default:
                            if (content.StartsWith("\\ansicpg"))
                            {
                                globalSettings[0] = Encoding.GetEncoding(int.Parse(content.Substring(8)));
                            }
                            break;
                    }
                sb.Append(before);
                if (idx > 0 && !comment)
                    sb.Append(content.Substring(idx + (content[idx] == ' ' ? 1 : 0)).Replace("\r\n", ""));
                RET:
                first = false;
                return;
            }
            //正式开始转换，初始默认环境是在某个{}内部
            bool qc = false;
            int i;
            int contentStart = startIndex;
            //以 \ 为分隔进行逐个识别
            for (i = startIndex; i < rtf.Length; ++i)
            {
                if (rtf[i] == '\\')
                {
                    if (qc = !qc)
                    {
                        add(rtf.Substring(contentStart, i - contentStart));
                        contentStart = i;
                    }
                }
                else
                {
                    //前一个字符不是\
                    if (!qc)
                    {
                        //rtf起始
                        if (rtf[i] == '{')
                        {
                            add(rtf.Substring(contentStart, i - contentStart));
                            //进入内部内容前清空残留解析内容
                            if (data.Count > 0)
                            {
                                sb.Append(globalSettings[0].GetString(data.ToArray()));
                                data.Clear();
                            }
                            //递归内部解析
                            int len = RtfToTxt(rtf, sb, i + 1, comment, globalSettings);
                            i += len + 1;
                            contentStart = i + 1;
                        }
                        else if (rtf[i] == '}')
                        {
                            add(rtf.Substring(contentStart, i - contentStart));
                            //退出前清空残留解析内容
                            if (data.Count > 0)
                                sb.Append(globalSettings[0].GetString(data.ToArray()));
                            break;
                        }
                    }
                    qc = false;
                }
            }
            return i - startIndex;
        }

    }
}
