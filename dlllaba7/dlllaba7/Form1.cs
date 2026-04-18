using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace dlllaba7
{
    public partial class Form1 : Form
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr LoadLibrary(string lpFileName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool FreeLibrary(IntPtr hModule);

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            //  WinAPI загрузка
            IntPtr handle = LoadLibrary("MyLibrary1.dll");

            if (handle == IntPtr.Zero)
            {
                lblStatus.Text = " DLL не загрузилась";
                return;
            }

            lblStatus.Text = " DLL загружена через WinAPI";

            try
            {
                //  вызов через Reflection
                var asm = Assembly.LoadFrom("MyLibrary1.dll");

                var type = asm.GetType("MyLibrary1.MyClass");
                var obj = Activator.CreateInstance(type);
                var method = type.GetMethod("ShowFormAndCalculate");

                var result = method.Invoke(obj, null);

                labelResult.Text = $"Результат: {result}";
            }
            catch (Exception ex)
            {
                lblStatus.Text = $"Ошибка: {ex.Message}";
            }
            finally
            {
                // выгрузка DLL
                FreeLibrary(handle);
                lblStatus.Text = "DLL выгружена";
            }
        }
    }
}