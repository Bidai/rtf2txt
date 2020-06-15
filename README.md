# rtf2txt

#### 简介
主要函数为RtfToTxt，功能为识别RTF格式内容中的文本

#### 函数
public static string RtfToTxt(string rtf);
[参数rtf] RTF格式的文本
[返回] 识别到的纯文本内容

#### 测试代码
``` C#
namespace RtfToTxt
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
        //简单的 txt->Rtf->txt 测试，跟利用System.Windows.Forms.RichTextBox转换的结果进行比较
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
```

#### 输出结果

``` C#
RTF文本：
{\rtf1\ansi\ansicpg936\deff0\nouicompat\deflang1033\deflangfe2052{\fonttbl{\f0\fnil\fcharset134 \'cb\'ce\'cc\'e5;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard\f0\fs18\lang2052\'18a\'16n,"?t\'04:3Fo\'04s@/"\'03\'086i+ NOP\'1c\}\'1dc\'1d\par
}


原始文本：
an,"?t:3Fos@/6i+ NOP}c
识别文本：
an,"?t:3Fos@/6i+ NOP}c

相等比较：True

RTF文本：
{\rtf1\ansi\ansicpg936\deff0\nouicompat\deflang1033\deflangfe2052{\fonttbl{\f0\fnil\fcharset134 \'cb\'ce\'cc\'e5;}}
{\*\generator Riched20 10.0.19041}\viewkind4\uc1
\pard\f0\fs18\lang2052\'b2\'e2\'ca\'d4\par
\\\tab\par
"\'d7\'d6\'b7\'fb\'b4\'ae'\par
}


原始文本：
\
"字符串'
识别文本：
测试
\
"字符串'

相等比较：True
```