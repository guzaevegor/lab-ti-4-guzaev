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
                string filePath = ""; // Переменная для пути к файлу

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

                // Всегда показываем диалог выбора файла
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Выберите файл для подписи"
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    filePath = openFileDialog.FileName;
                    sourceBytes = File.ReadAllBytes(filePath);
                    FileContentTextBox.Text = Encoding.UTF8.GetString(sourceBytes);
                    SelectedFileLabel.Content = filePath; // Сохраняем полный путь
                    AddToLog($"Загружен файл: {filePath}");
                }
                else
                {
                    return; // Пользователь отменил выбор файла
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

                // Формируем блок подписи
                string signatureBlock =
                    "-----BEGIN DSA SIGNATURE-----\n" +
                    $"r={r}\n" +
                    $"s={s}\n" +
                    $"p={p}\n" +
                    $"q={q}\n" +
                    $"g={g}\n" +
                    $"y={y}\n" +
                    "-----END DSA SIGNATURE-----\n\n";

                // Добавляем подпись в начало исходного файла
                string originalContent = File.ReadAllText(filePath);
                string signedContent = signatureBlock + originalContent;

                // Сохраняем подписанный файл
                File.WriteAllText(filePath, signedContent);
                AddToLog($"Подпись добавлена в начало исходного файла: {filePath}");
                StatusTextBlock.Text = "Подпись успешно сгенерирована и добавлена в файл";

                // Обновляем отображаемое содержимое файла
                FileContentTextBox.Text = signedContent;
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
            // Очищаем предыдущие результаты проверки
            VerificationResultTextBox.Text = "";

            try
            {
                // Показываем диалог выбора файла с подписью
                OpenFileDialog openFileDialog = new OpenFileDialog
                {
                    Filter = "Текстовые файлы (*.txt)|*.txt|Все файлы (*.*)|*.*",
                    Title = "Выберите подписанный файл для проверки"
                };

                if (openFileDialog.ShowDialog() != true)
                    return; // Пользователь отменил выбор файла

                string filePath = openFileDialog.FileName;
                string fileContent = File.ReadAllText(filePath);
                AddToLog($"Загружен файл для проверки: {filePath}");

                // Ищем начало и конец блока подписи (в начале файла)
                int signatureStart = fileContent.IndexOf("-----BEGIN DSA SIGNATURE-----");
                int signatureEnd = fileContent.IndexOf("-----END DSA SIGNATURE-----");

                if (signatureStart != 0)
                {
                    throw new Exception("Файл не содержит блока подписи DSA в начале");
                }

                if (signatureStart < 0 || signatureEnd < 0 || signatureEnd <= signatureStart)
                {
                    throw new Exception("Файл не содержит корректного блока подписи DSA");
                }

                // Конец блока подписи включает длину маркера окончания
                int endOfSignatureBlock = signatureEnd + "-----END DSA SIGNATURE-----".Length;

                // Извлекаем оригинальный контент после блока подписи
                string originalContent = fileContent.Substring(endOfSignatureBlock).TrimStart();
                byte[] originalBytes = Encoding.UTF8.GetBytes(originalContent);

                // Извлекаем блок подписи
                string signatureBlock = fileContent.Substring(
                    signatureStart + "-----BEGIN DSA SIGNATURE-----".Length,
                    signatureEnd - signatureStart - "-----BEGIN DSA SIGNATURE-----".Length
                );

                // Парсим параметры подписи
                Dictionary<string, int> signatureParams = new Dictionary<string, int>();
                foreach (string line in signatureBlock.Split('\n'))
                {
                    string trimmedLine = line.Trim();
                    if (string.IsNullOrEmpty(trimmedLine)) continue;

                    string[] parts = trimmedLine.Split('=');
                    if (parts.Length == 2)
                    {
                        signatureParams[parts[0].Trim()] = int.Parse(parts[1].Trim());
                    }
                }

                // Извлекаем параметры подписи
                int r = signatureParams["r"];
                int s = signatureParams["s"];
                int p = signatureParams["p"];
                int q = signatureParams["q"];
                int g = signatureParams["g"];
                int y = signatureParams["y"];

                AddToLog($"Извлечены параметры подписи: r={r}, s={s}, p={p}, q={q}, g={g}, y={y}");

                // Отображаем содержимое файла и параметры подписи в интерфейсе
                FileContentTextBox.Text = originalContent;
                SelectedFileLabel.Content = filePath;
                RValueTextBox.Text = r.ToString();
                SValueTextBox.Text = s.ToString();
                PTextBox.Text = p.ToString();
                QTextBox.Text = q.ToString();
                GValueTextBox.Text = g.ToString();
                YValueTextBox.Text = y.ToString();

                // Вычисляем хеш сообщения
                int hash = CalculateHash(originalBytes, q);
                HashValueTextBox.Text = hash.ToString();
                AddToLog($"Вычислен хеш сообщения: {hash}");

                // Проверяем подпись
                bool isValid = VerifyDSASignature(originalBytes, r, s, p, q, g, y);

                if (isValid)
                {
                    VerificationResultTextBox.Text = "ПОДПИСЬ ВЕРНА";
                    VerificationResultTextBox.Background = new SolidColorBrush(Colors.LightGreen);
                    AddToLog("Результат проверки: ПОДПИСЬ ВЕРНА");
                    StatusTextBlock.Text = "Проверка подписи: ПОДПИСЬ ВЕРНА";
                }
                else
                {
                    VerificationResultTextBox.Text = "ПОДПИСЬ НЕВЕРНА";
                    VerificationResultTextBox.Background = new SolidColorBrush(Colors.LightPink);
                    AddToLog("Результат проверки: ПОДПИСЬ НЕВЕРНА");
                    StatusTextBlock.Text = "Проверка подписи: ПОДПИСЬ НЕВЕРНА";
                }

                // Обновляем sourceBytes для возможности повторной подписи того же файла
                sourceBytes = originalBytes;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка проверки подписи: {ex.Message}", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                AddToLog($"Ошибка: {ex.Message}");
                StatusTextBlock.Text = "Ошибка проверки подписи";

                VerificationResultTextBox.Text = "ОШИБКА ПРОВЕРКИ";
                VerificationResultTextBox.Background = new SolidColorBrush(Colors.LightPink);
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
