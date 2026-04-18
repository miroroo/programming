using System;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Forms;

namespace MyLibrary1
{
    public class MyClass
    {
        public double ShowFormAndCalculate()
        {
            double result = 0;

            Form form = new Form();
            form.Text = "Введите число";
            form.Width = 350;
            form.Height = 220;

            // Label for formula
            System.Windows.Forms.Label labelFormula = new System.Windows.Forms.Label();
            labelFormula.Text = "Формула: f(x, y) = (x-3)^5 - y";
            labelFormula.Top = 10;
            labelFormula.Left = 10;
            labelFormula.Width = 300;

            // Label for X
            System.Windows.Forms.Label labelX = new System.Windows.Forms.Label();
            labelX.Text = "Введите х:";
            labelX.Top = 40;
            labelX.Left = 10;
            labelX.Width = 70;

            // NumericUpDown for X
            NumericUpDown numX = new NumericUpDown();
            numX.Top = 40;
            numX.Left = 100;
            numX.Width = 150;
            numX.Minimum = -1000;
            numX.Maximum = 1000;

            // Label for Y
            System.Windows.Forms.Label labelY = new System.Windows.Forms.Label();
            labelY.Text = "Введите у:";
            labelY.Top = 70;
            labelY.Left = 10;
            labelY.Width = 70;

            // NumericUpDown for Y
            NumericUpDown numY = new NumericUpDown();
            numY.Top = 70;
            numY.Left = 100;
            numY.Width = 150;
            numY.Minimum = -1000;
            numY.Maximum = 1000;

            // Label for res
            System.Windows.Forms.Label labelRes = new System.Windows.Forms.Label();
            labelRes.Text = "Результат:";
            labelRes.Top = 120;
            labelRes.Left = 200;
            labelRes.Width = 140;

            // Button
            Button btnOK = new Button();
            btnOK.Text = "Показать результат";
            btnOK.Top = 120;
            btnOK.Left = 10;
            btnOK.Width = 140;

            Button btnClose = new Button();
            btnClose.Text = "Показать результат в главном приложении";
            btnClose.Top = 150;
            btnClose.Left = 10;
            btnClose.Width = 300;


            btnOK.Click += (s, e) =>
            {
              try
              {
                    double x = (double)numX.Value;
                    double y = (double)numY.Value;

                    var asm = Assembly.LoadFrom("interLibrary1.dll");
                    var type = asm.GetType("interLibrary1.interClass");
                    var obj = Activator.CreateInstance(type);
                    var method = type.GetMethod("Calculate");
                    var res = method.Invoke(obj, new object[] { x, y });

                    result = Convert.ToDouble(res);
                    labelRes.Text = $"Результат: {result}";
              }
              catch (Exception ex)
              {
                  MessageBox.Show(ex.Message);
              }
            };

            btnClose.Click += (s, e) =>
            {
                double x = (double)numX.Value;
                double y = (double)numY.Value;

                var asm = Assembly.LoadFrom("interLibrary1.dll");
                var type = asm.GetType("interLibrary1.interClass");
                var obj = Activator.CreateInstance(type);
                var method = type.GetMethod("Calculate");
                var res = method.Invoke(obj, new object[] { x, y });

                result = Convert.ToDouble(res);
                form.Close();
            };

            form.Controls.Add(labelFormula);
            form.Controls.Add(labelX);
            form.Controls.Add(numX);
            form.Controls.Add(labelY);
            form.Controls.Add(numY);
            form.Controls.Add(btnOK);
            form.Controls.Add(btnClose);
            form.Controls.Add(labelRes);

            form.ShowDialog();

            return result;
        }
    }
}