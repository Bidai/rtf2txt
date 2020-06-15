using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TtfToTxt
{
    class Program
    {
        static void Main(string[] args)
        {

            Random r = new Random();
            char[] chs = new char[32];
            for (int i = 0; i < chs.Length; ++i)
                chs[i] = (char)r.Next(1, 127);

            Test(new string(chs));//随机字符串
            Test("测试\r\\\t\n\"字符串\'");//可以换成其他内容测试

            Console.Read();
        }
        //简单的 txt->Rtf->txt 测试
        static void Test(string txt) {
            System.Windows.Forms.RichTextBox rtb = new System.Windows.Forms.RichTextBox();
            rtb.Text = txt;
            //通过RichTextBox转为Rtf格式文本
            var result = rtf2txt.RtfToTxt(rtb.Rtf);

            Console.WriteLine("RTF文本：\n" + rtb.Rtf + "\n");

            Console.WriteLine("原始文本：\n" + txt);
            Console.WriteLine("识别文本：\n" + result);
            Console.WriteLine("\n相等比较：" + (rtb.Text == result)+"\n");
        }
    }
}
