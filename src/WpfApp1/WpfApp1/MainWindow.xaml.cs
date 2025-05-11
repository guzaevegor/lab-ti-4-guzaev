using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;

namespace DSASignatureApp
{
    public partial class MainWindow : Window
    {
        private byte[] sourceBytes;

        public MainWindow()
        {
            InitializeComponent();
        }

        #region Валидация параметров

        private void QTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(QTextBox.Text, out int value))
            {
                if (IsPrime(value))
                {
                    QValidationLabel.Content = "q является простым числом";
                    QValidationLabel.Foreground = Brushes.Green;
                }
                else
                {
                    QValidationLabel.Content = "q должно быть простым числом";
                    QValidationLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                QValidationLabel.Content = "Введите целое число q";
                QValidationLabel.Foreground = Brushes.Gray;
            }

            UpdateParameters();
        }

        private void PTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(PTextBox.Text, out int valueP) && int.TryParse(QTextBox.Text, out int valueQ))
            {
                if (IsPrime(valueP))
                {
                    if ((valueP - 1) % valueQ == 0)
                    {
                        PValidationLabel.Content = "p является простым числом и (p-1) делится на q";
                        PValidationLabel.Foreground = Brushes.Green;
                    }
                    else
                    {
                        PValidationLabel.Content = "p является простым числом, но (p-1) не делится на q";
                        PValidationLabel.Foreground = Brushes.Red;
                    }
                }
                else
                {
                    PValidationLabel.Content = "p должно быть простым числом";
                    PValidationLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                PValidationLabel.Content = "Введите целое число p";
                PValidationLabel.Foreground = Brushes.Gray;
            }

            UpdateParameters();
        }

        private void HTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(HTextBox.Text, out int valueH) && int.TryParse(PTextBox.Text, out int valueP))
            {
                if (valueH > 1 && valueH < valueP - 1)
                {
                    HValidationLabel.Content = "h находится в допустимом диапазоне (1, p-1)";
                    HValidationLabel.Foreground = Brushes.Green;
                }
                else
                {
                    HValidationLabel.Content = "h должно быть в диапазоне (1, p-1)";
                    HValidationLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                HValidationLabel.Content = "Введите целое число h";
                HValidationLabel.Foreground = Brushes.Gray;
            }

            UpdateParameters();
        }

        private void XTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(XTextBox.Text, out int valueX) && int.TryParse(QTextBox.Text, out int valueQ))
            {
                if (valueX > 0 && valueX < valueQ)
                {
                    XValidationLabel.Content = "x находится в допустимом диапазоне (0, q)";
                    XValidationLabel.Foreground = Brushes.Green;
                }
                else
                {
                    XValidationLabel.Content = "x должно быть в диапазоне (0, q)";
                    XValidationLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                XValidationLabel.Content = "Введите целое число x";
                XValidationLabel.Foreground = Brushes.Gray;
            }

            UpdateParameters();
        }

        private void KTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(KTextBox.Text, out int valueK) && int.TryParse(QTextBox.Text, out int valueQ))
            {
                if (valueK > 0 && valueK < valueQ)
                {
                    KValidationLabel.Content = "k находится в допустимом диапазоне (0, q)";
                    KValidationLabel.Foreground = Brushes.Green;
                }
                else
                {
                    KValidationLabel.Content = "k должно быть в диапазоне (0, q)";
                    KValidationLabel.Foreground = Brushes.Red;
                }
            }
            else
            {
                KValidationLabel.Content = "Введите целое число k";
                KValidationLabel.Foreground = Brushes.Gray;
            }
        }

        private void UpdateParameters()
        {
            try
            {
                if (int.TryParse(PTextBox.Text, out int p) &&
                    int.TryParse(QTextBox.Text, out int q) &&
                    int.TryParse(HTextBox.Text, out int h) &&
                    IsPrime(p) && IsPrime(q) && (p - 1) % q == 0 && h > 1 && h < p - 1)
                {
                    // Вычисляем g = h^((p-1)/q) mod p
                    int g = CalculateG(p, q, h);
                    GValueTextBox.Text = g.ToString();

                    // Если x валидный, вычисляем y = g^x mod p
                    if (int.TryParse(XTextBox.Text, out int x) && x > 0 && x < q)
                    {
                        int y = ModPow(g, x, p);
                        YValueTextBox.Text = y.ToString();
                    }
                }
            }
            catch (Exception ex)
            {
                StatusTextBlock.Text = $"Ошибка: {ex.Message}";
            }
        }

        #endregion

        #region Алгоритм DSA

        // Проверка на простое число
        private bool IsPrime(int number)
        {
            if (number <= 1) return false;
            if (number <= 3) return true;
            if (number % 2 == 0 || number % 3 == 0) return false;

            for (int i = 5; i * i <= number; i += 6)
            {
                if (number % i == 0 || number % (i + 2) == 0)
                    return false;
            }

            return true;
        }

        // Вычисление g = h^((p-1)/q) mod p
        private int CalculateG(int p, int q, int h)
        {
            if (h <= 1 || h >= p)
                throw new ArgumentException("h должно быть в диапазоне (1, p-1)");

            int exponent = (p - 1) / q;
            int g = ModPow(h, exponent, p);

            if (g <= 1)
                throw new Exception("Некорректное значение g. Попробуйте другое значение h.");

            return g;
        }

        // Быстрое возведение в степень по модулю
        private int ModPow(int baseValue, int exponent, int modulus)
        {
            if (modulus == 1) return 0;

            BigInteger result = BigInteger.ModPow(baseValue, exponent, modulus);
            return (int)result;
        }

        // Вычисление хеш-образа по функции 3.2
        private int CalculateHash(byte[] message, int q, int initialValue = 100)
        {
            int hash = initialValue;

            foreach (byte b in message)
            {
                // H_i = (H_(i-1) + M_i)^2 mod q
                int previousHash = hash;
                hash = (int)((BigInteger.Pow(hash + b, 2)) % q);
                AddToLog($"Шаг хеширования: ({previousHash} + {b})^2 mod {q} = {hash}");

            }

            return hash;
        }

        // Генерация подписи DSA
        private (int r, int s) GenerateDSASignature(byte[] message, int p, int q, int g, int x, int k)
        {
            // Вычисляем хеш сообщения
            int hash = CalculateHash(message, q);

            // Вычисляем r = (g^k mod p) mod q
            int r = ModPow(g, k, p) % q;
            AddToLog($"Вычислено r = (g^k mod p) mod q = ({g}^{k} mod {p}) mod {q} = {r}");

            if (r == 0)
            {
                throw new Exception("r равно 0, требуется другое значение k");
            }

            // Вычисляем k^-1 mod q используя малую теорему Ферма: k^-1 = k^(q-2) mod q
            int kInverse = ModPow(k, q - 2, q);
            AddToLog($"Вычислено k^-1 mod q = {k}^{q - 2} mod {q} = {kInverse}");

            // Вычисляем s = k^-1 * (hash + x*r) mod q
            int s = (int)(((long)kInverse * (hash + (long)x * r)) % q);
            AddToLog($"Вычислено s = k^-1 * (hash + x*r) mod q = {kInverse} * ({hash} + {x}*{r}) mod {q} = {s}");

            if (s == 0)
            {
                throw new Exception("s равно 0, требуется другое значение k");
            }

            return (r, s);
        }

        // Проверка подписи DSA
        private bool VerifyDSASignature(byte[] message, int r, int s, int p, int q, int g, int y)
        {
            // Проверяем, что 0 < r < q и 0 < s < q
            if (r <= 0 || r >= q || s <= 0 || s >= q)
            {
                AddToLog("Недействительная подпись: значения r или s выходят за допустимый диапазон");
                return false;
            }

            // Вычисляем хеш сообщения
            int hash = CalculateHash(message, q);

            // Вычисляем w = s^-1 mod q используя малую теорему Ферма: s^-1 = s^(q-2) mod q
            int w = ModPow(s, q - 2, q);
            AddToLog($"Вычислено w = s^-1 mod q = {s}^{q - 2} mod {q} = {w}");

            // Вычисляем u1 = hash * w mod q
            int u1 = (int)(((long)hash * w) % q);
            AddToLog($"Вычислено u1 = hash * w mod q = {hash} * {w} mod {q} = {u1}");

            // Вычисляем u2 = r * w mod q
            int u2 = (int)(((long)r * w) % q);
            AddToLog($"Вычислено u2 = r * w mod q = {r} * {w} mod {q} = {u2}");

            // Вычисляем v = (g^u1 * y^u2 mod p) mod q
            BigInteger v1 = BigInteger.ModPow(g, u1, p);
            BigInteger v2 = BigInteger.ModPow(y, u2, p);
            int v = (int)((v1 * v2) % p % q);
            AddToLog($"Вычислено v = (g^u1 * y^u2 mod p) mod q = ({g}^{u1} * {y}^{u2} mod {p}) mod {q} = {v}");

            // Подпись действительна, если v = r
            return v == r;
        }

        #endregion

        #region Обработчики кнопок

        private void SignButton_Click(object sender, RoutedEventArgs e)
        {
            // Очищаем предыдущие результаты
            RValueTextBox.Text = "";
            SValueTextBox.Text = "";
            HashValueTextBox.Text = "";
            VerificationResultTextBox.Text = "";

            try
            {
                // Проверяем параметры
                if (!int.TryParse(QTextBox.Text, out int q) || !IsPrime(q))
                    throw new Exception("q должно быть простым числом");

                if (!int.TryParse(PTextBox.Text, out int p) || !IsPrime(p) || (p - 1) % q != 0)
                    throw new Exception("p должно быть простым числом и (p-1) должно делиться на q");

                if (!int.TryParse(HTextBox.Text, out int h) || h <= 1 || h >= p - 1)
                    throw new Exception("h должно быть в диапазоне (1, p-1)");

                if (!int.TryParse(XTextBox.Text, out int x) || x <= 0 || x >= q)
                    throw new Exception("x должно быть в диапазоне (0, q)");

                if (!int.TryParse(KTextBox.Text, out int k) || k <= 0 || k >= q)
                    throw new Exception("k должно быть в диапазоне (0, q)");

                // Проверяем, есть ли файл для подписи
                if (sourceBytes == null || sourceBytes.Length == 0)
                {
                    // Показываем диалог выбора файла
                    OpenFileDialog openFileDialog = new OpenFileDialog
                    {
                        Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                        Title = "Выберите файл для подписи"
                    };

                    if (openFileDialog.ShowDialog() == true)
                    {
                        string sourcePath = openFileDialog.FileName;
                        sourceBytes = File.ReadAllBytes(sourcePath);
                        FileContentTextBox.Text = Encoding.UTF8.GetString(sourceBytes);
                        SelectedFileLabel.Content = Path.GetFileName(sourcePath);
                        AddToLog($"Загружен файл: {sourcePath}");
                    }
                    else
                    {
                        return; // Пользователь отменил выбор файла
                    }
                }

                // Вычисляем g
                int g = CalculateG(p, q, h);
                GValueTextBox.Text = g.ToString();
                AddToLog($"Вычислено g = h^((p-1)/q) mod p = {h}^{(p - 1) / q} mod {p} = {g}");

                // Вычисляем y (открытый ключ)
                int y = ModPow(g, x, p);
                YValueTextBox.Text = y.ToString();
                AddToLog($"Вычислено y = g^x mod p = {g}^{x} mod {p} = {y}");

                // Вычисляем хеш
                int hash = CalculateHash(sourceBytes, q);
                HashValueTextBox.Text = hash.ToString();
                AddToLog($"Вычислен хеш сообщения: {hash}");

                // Генерируем подпись
                bool signatureGenerated = false;
                int r = 0, s = 0;

                while (!signatureGenerated)
                {
                    try
                    {
                        (r, s) = GenerateDSASignature(sourceBytes, p, q, g, x, k);
                        signatureGenerated = true;
                    }
                    catch (Exception ex)
                    {
                        // Если r или s равно 0, запрашиваем новое значение k
                        if (ex.Message.Contains("равно 0"))
                        {
                            AddToLog(ex.Message);

                            var result = MessageBox.Show(
                                $"{ex.Message}. Пожалуйста, введите новое значение k.",
                                "Требуется новое значение k",
                                MessageBoxButton.OKCancel);

                            if (result == MessageBoxResult.OK)
                            {
                                KTextBox.Focus();
                                return;
                            }
                            else
                            {
                                AddToLog("Подписывание отменено");
                                return;
                            }
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                // Отображаем значения подписи
                RValueTextBox.Text = r.ToString();
                SValueTextBox.Text = s.ToString();

                AddToLog($"Сгенерирована подпись: r = {r}, s = {s}");

                // Сохраняем подпись в отдельный файл .dsa
                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Файлы подписи DSA (*.dsa)|*.dsa|Все файлы (*.*)|*.*",
                    Title = "Сохранить файл подписи",
                    // Устанавливаем то же имя файла, что и у исходного, но с расширением .dsa
                    FileName = Path.ChangeExtension(SelectedFileLabel.Content.ToString(), ".dsa")
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    // Создаем содержимое файла подписи - только значения r и s
                    string signatureContent = $"{r}\n{s}\n{p}\n{q}\n{g}\n{y}";
                    File.WriteAllText(saveFileDialog.FileName, signatureContent);
                    AddToLog($"Файл подписи сохранен: {saveFileDialog.FileName}");
                    StatusTextBlock.Text = "Подпись успешно сгенерирована и сохранена";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка генерации подписи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AddToLog($"Ошибка: {ex.Message}");
                StatusTextBlock.Text = "Ошибка генерации подписи";
            }
        }


        private void VerifyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Запрашиваем у пользователя выбор исходного файла для проверки
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Выберите исходный файл для проверки"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string sourceFilePath = openFileDialog.FileName;
                    byte[] fileBytes = File.ReadAllBytes(sourceFilePath);
                    FileContentTextBox.Text = Encoding.UTF8.GetString(fileBytes);
                    AddToLog($"Загружен исходный файл для проверки: {sourceFilePath}");

                    // Теперь запрашиваем файл подписи
                    OpenFileDialog signatureDialog = new OpenFileDialog
                    {
                        Filter = "Файлы подписи DSA (*.dsa)|*.dsa|Все файлы (*.*)|*.*",
                        Title = "Выберите файл подписи",
                        // Предлагаем файл подписи с тем же именем, но другим расширением
                        FileName = Path.ChangeExtension(sourceFilePath, ".dsa")
                    };

                    if (signatureDialog.ShowDialog() == true)
                    {
                        string signatureFilePath = signatureDialog.FileName;

                        // Читаем значения r, s, p, q, g, y из файла подписи
                        string[] signatureLines = File.ReadAllLines(signatureFilePath);

                        if (signatureLines.Length >= 6 &&
                            int.TryParse(signatureLines[0], out int r) &&
                            int.TryParse(signatureLines[1], out int s) &&
                            int.TryParse(signatureLines[2], out int p) &&
                            int.TryParse(signatureLines[3], out int q) &&
                            int.TryParse(signatureLines[4], out int g) &&
                            int.TryParse(signatureLines[5], out int y))
                        {
                            // Заполняем поля значениями из файла подписи
                            PTextBox.Text = p.ToString();
                            QTextBox.Text = q.ToString();
                            GValueTextBox.Text = g.ToString();
                            YValueTextBox.Text = y.ToString();
                            RValueTextBox.Text = r.ToString();
                            SValueTextBox.Text = s.ToString();

                            // Вычисляем хеш исходного файла
                            int hash = CalculateHash(fileBytes, q);
                            HashValueTextBox.Text = hash.ToString();
                            AddToLog($"Вычислен хеш для проверки: {hash}");

                            // Проверяем подпись
                            bool isValid = VerifyDSASignature(fileBytes, r, s, p, q, g, y);

                            // Вычисляем значения для проверки
                            int w = ModPow(s, q - 2, q);
                            int u1 = (int)(((long)hash * w) % q);
                            int u2 = (int)(((long)r * w) % q);
                            BigInteger v1 = BigInteger.ModPow(g, u1, p);
                            BigInteger v2 = BigInteger.ModPow(y, u2, p);
                            int v = (int)((v1 * v2) % p % q);

                            // Отображаем результат проверки
                            string resultMessage = isValid ? "Подпись верна" : "Подпись неверна";
                            VerificationResultTextBox.Text = resultMessage;
                            VerificationResultTextBox.Foreground = isValid ? Brushes.Green : Brushes.Red;

                            // Логируем детали проверки
                            AddToLog($"Проверка w = {w}");
                            AddToLog($"Проверка u1 = {u1}");
                            AddToLog($"Проверка u2 = {u2}");
                            AddToLog($"Проверка v = {v}");
                            AddToLog($"Ожидаемое r = {r}");
                            AddToLog($"Результат проверки: {resultMessage}");

                            MessageBox.Show(
                                $"Результат проверки: {resultMessage}\n\n" +
                                $"Вычисленные значения:\n" +
                                $"w = {w}\n" +
                                $"u1 = {u1}\n" +
                                $"u2 = {u2}\n" +
                                $"v = {v}\n" +
                                $"r = {r}",
                                "Проверка подписи",
                                MessageBoxButton.OK,
                                isValid ? MessageBoxImage.Information : MessageBoxImage.Warning);

                            StatusTextBlock.Text = "Проверка завершена";
                        }
                        else
                        {
                            throw new Exception("Некорректный формат файла подписи");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки подписи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AddToLog($"Ошибка: {ex.Message}");
                StatusTextBlock.Text = "Ошибка проверки подписи";
            }
        }

        private void SelectFileButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                Title = "Выберите файл"
            };

            if (openFileDialog.ShowDialog() == true)
            {
                sourceBytes = File.ReadAllBytes(openFileDialog.FileName);
                FileContentTextBox.Text = Encoding.UTF8.GetString(sourceBytes);
                AddToLog($"Загружен файл: {openFileDialog.FileName}");
                StatusTextBlock.Text = "Файл успешно загружен";
                SelectedFileLabel.Content = Path.GetFileName(openFileDialog.FileName);
                // Очищаем предыдущие результаты подписи
                RValueTextBox.Text = "";
                SValueTextBox.Text = "";
                HashValueTextBox.Text = "";
                VerificationResultTextBox.Text = "";
            }
        }

        #endregion

        #region Обработчики меню

        private void OpenMenuItem_Click(object sender, RoutedEventArgs e)
        {
            SelectFileButton_Click(sender, e);
        }

        private void SaveMenuItem_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(RValueTextBox.Text) || string.IsNullOrEmpty(SValueTextBox.Text))
            {
                MessageBox.Show("Подпись ещё не сгенерирована", "Информация", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Создаем подписанное сообщение
                string signedMessage = FileContentTextBox.Text +
                    $"\r\n\r\nЦифровая подпись DSA:\r\nr = {RValueTextBox.Text}\r\ns = {SValueTextBox.Text}";

                SaveFileDialog saveFileDialog = new SaveFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Сохранить подписанное сообщение"
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveFileDialog.FileName, signedMessage);
                    AddToLog($"Подписанное сообщение сохранено в: {saveFileDialog.FileName}");
                    StatusTextBlock.Text = "Подписанное сообщение успешно сохранено";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения подписанного сообщения: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AddToLog($"Ошибка: {ex.Message}");
            }
        }

        private void ExitMenuItem_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void AboutMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("DSA Цифровая Подпись\nПриложение для генерации и проверки цифровых подписей с использованием алгоритма DSA.",
                "О программе", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void DocumentationMenuItem_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "Параметры DSA цифровой подписи:\n\n" +
                "q - простое число\n" +
                "p - простое число такое, что (p-1) делится на q\n" +
                "h - целое число в диапазоне (1, p-1)\n" +
                "g = h^((p-1)/q) mod p\n" +
                "x - закрытый ключ в диапазоне (0, q)\n" +
                "y = g^x mod p - открытый ключ\n" +
                "k - случайное целое число в диапазоне (0, q) для генерации подписи\n\n" +
                "Генерация подписи:\n" +
                "r = (g^k mod p) mod q\n" +
                "s = k^-1 * (hash + x*r) mod q\n\n" +
                "Проверка подписи:\n" +
                "w = s^-1 mod q\n" +
                "u1 = hash * w mod q\n" +
                "u2 = r * w mod q\n" +
                "v = (g^u1 * y^u2 mod p) mod q\n" +
                "Подпись верна, если v = r",
                "Документация", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Вспомогательные методы

        private void AddToLog(string message)
        {
            LogItemsControl.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        #endregion
    }

    // Вспомогательный класс для диалогов ввода
    public class InputDialog : Window
    {
        private TextBox textBox1;
        private TextBox textBox2;

        public string Value1 { get; private set; }
        public string Value2 { get; private set; }

        public InputDialog(string title, string prompt1, string prompt2)
        {
            Title = title;
            Width = 300;
            Height = prompt2.Length > 0 ? 200 : 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;

            Grid grid = new Grid();
            grid.Margin = new Thickness(10);

            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

            TextBlock textBlock1 = new TextBlock { Text = prompt1, Margin = new Thickness(0, 0, 0, 5) };
            grid.Children.Add(textBlock1);
            Grid.SetRow(textBlock1, 0);

            textBox1 = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            grid.Children.Add(textBox1);
            Grid.SetRow(textBox1, 1);

            if (prompt2.Length > 0)
            {
                TextBlock textBlock2 = new TextBlock { Text = prompt2, Margin = new Thickness(0, 0, 0, 5) };
                grid.Children.Add(textBlock2);
                Grid.SetRow(textBlock2, 2);

                textBox2 = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
                grid.Children.Add(textBox2);
                Grid.SetRow(textBox2, 3);
            }

            StackPanel buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };

            Button okButton = new Button { Content = "OK", Width = 60, Margin = new Thickness(0, 0, 10, 0) };
            okButton.Click += OkButton_Click;
            buttonPanel.Children.Add(okButton);

            Button cancelButton = new Button { Content = "Отмена", Width = 60 };
            cancelButton.Click += CancelButton_Click;
            buttonPanel.Children.Add(cancelButton);

            grid.Children.Add(buttonPanel);
            Grid.SetRow(buttonPanel, prompt2.Length > 0 ? 4 : 2);

            Content = grid;
        }

        private void OkButton_Click(object sender, RoutedEventArgs e)
        {
            Value1 = textBox1.Text;
            Value2 = textBox2?.Text ?? "";
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
