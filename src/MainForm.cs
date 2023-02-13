﻿using Microsoft.Win32;

using Shark_Remote.Engine.Bot;
using Shark_Remote.Helpers;

using System.Drawing.Printing;
using System.Globalization;
using System.IO.Compression;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text.RegularExpressions;

using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Extensions.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.InputFiles;
using Telegram.Bot.Types.ReplyMarkups;

using Tommy;

using VitNX3.Functions.Win32;
using VitNX3.Functions.WinControllers;

using static Shark_Remote.Engine.API.Functions;

using Constants = VitNX3.Functions.Win32.Constants;
using DateTime = System.DateTime;
using Dir = VitNX3.Functions.FileSystem.Folder;
using File = VitNX3.Functions.FileSystem.File;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Processes = VitNX3.Functions.AppsAndProcesses.Processes;
using SysFile = System.IO.File;

namespace Shark_Remote
{
    public partial class MainForm : Form
    {
        public bool
            taskMgrOff = true;

        public string
            tempPath = $@"{Application.StartupPath}\data\temp",
            ip = VitNX3.Functions.Information.Internet.GetPublicIP(),
            tfileStr;

        public string[]
            plugins,
            pluginsAction;

        private VitNX3.Functions.SettingsAndLog.Log log = new VitNX3.Functions.SettingsAndLog.Log($@"{Application.StartupPath}\data\shark_remote.log");

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= Constants.CS_DROPSHADOW;
                return cp;
            }
        }

        public MainForm()
        {
            InitializeComponent();
            KeyPreview = true;
            versionLabel.Text = versionLabelText;
            ServicePointManager.SecurityProtocol = VitNX3.Functions.Web.Config.UseProtocols();
            Import.SetProcessDpiAwareness(Enums.PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE);
            Import.ReleaseCapture();
            if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == true)
            {
                try
                {
                    if (Convert.ToInt64(VitNX3.Functions.Information.Windows.GetWindowsCurrentBuildNumberFromRegistry()) >= 2200)
                        Region = VitNX3.Functions.WindowAndControls.Window.SetWindowsElevenStyleForWinForm(Handle, Width, Height);
                    else
                        Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
                }
                catch { Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15)); }
            }
            Home.MouseEnter += new EventHandler(UI.MyButton_MouseEnter);
            Home.MouseLeave += new EventHandler(UI.MyButton_MouseLeave);
            Settings.MouseEnter += new EventHandler(UI.MyButton_MouseEnter);
            Settings.MouseLeave += new EventHandler(UI.MyButton_MouseLeave);
            Plugins.MouseEnter += new EventHandler(UI.MyButton_MouseEnter);
            Plugins.MouseLeave += new EventHandler(UI.MyButton_MouseLeave);
            Help.MouseEnter += new EventHandler(UI.MyButton_MouseEnter);
            Help.MouseLeave += new EventHandler(UI.MyButton_MouseLeave);
            GetSettings();
            Import.SetThreadExecutionState(Enums.EXECUTION_STATE.ES_CONTINUOUS |
                Enums.EXECUTION_STATE.ES_SYSTEM_REQUIRED |
                Enums.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
            if (Network.InternetOk() == false)
            {
                if (AppValues.serviceMode)
                    Console.WriteLine("Cannot startup because no Internet connection was detected!");
                else
                    VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Нет доступа в сеть!",
                        "Ошибка соединения",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                log.Write("Нет доступа в сеть!");
                Processes.KillNative($"Shark Remote.exe");
            }
            botPowerControl.Checked = true;
            if (!Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
            {
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                {
                    Opacity = 0;
                    System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                    launch.Tick += new EventHandler((sender, e) =>
                    {
                        if ((Opacity += 0.05d) == 1)
                            launch.Stop();
                    });
                    launch.Interval = 20;
                    launch.Start();
                }
            }
            else
                Opacity = 0.96;
            if (AppValues.botToken == "")
            {
                botPowerControlStatus.ForeColor = Color.FromArgb(215, 99, 90);
                botPowerControlStatus.Texts = "Деактивирован";
                statusBot.Text = "Откл.";
                eventsLog.Items.Clear();
                AddEvent("Ожидаю включения...");
            }
            botPowerControlStatus.ReadOnly = true;
            botName.ReadOnly = true;
            botId.ReadOnly = true;
            botUsername.ReadOnly = true;
        }



        protected override void OnPaint(PaintEventArgs e)
        {
            if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == false)
            {
                base.OnPaint(e);
                Graphics g = e.Graphics;
                Rectangle rect = new Rectangle(new Point(0, 0), new Size(Width, Height));
                Pen pen = new Pen(Color.FromArgb(26, 32, 48));
                g.DrawRectangle(pen, rect);
            }
        }

        private void GetSettings(bool readUiSettings = true)
        {
            AppValues.botToken = AppSettings.Read("bot", "token");
            username.Texts = AppSettings.Read("bot", "username");
            if (AppValues.botToken.Length < 15)
                AppValues.botToken = "";
            if (readUiSettings)
            {
                if (AppSettings.Read("ui", "menu_color") == "default")
                    selectedMenu.BackColor = Color.FromArgb(33, 61, 92);
                if (AppSettings.Read("ui", "menu_color") == "keyboard")
                    selectedMenu.BackColor = Color.FromArgb(214, 222, 228);
                if (AppSettings.Read("ui", "menu_color") == "unigram")
                    selectedMenu.BackColor = Color.FromArgb(69, 105, 147);
                if (AppSettings.Read("ui", "menu_color") == "vivaldi")
                    selectedMenu.BackColor = Color.FromArgb(234, 56, 56);
                if (AppSettings.Read("ui", "menu_color") == "github")
                    selectedMenu.BackColor = Color.FromArgb(117, 47, 156);
                if (AppSettings.Read("ui", "menu_color") == "μtorrent")
                    selectedMenu.BackColor = Color.FromArgb(141, 196, 95);
                if (AppSettings.Read("ui", "menu_color") == "native")
                    selectedMenu.BackColor = VitNX3.Functions.Information.Windows.GetWindowsAccentColor();
                try { AppValues.miniMode = Convert.ToBoolean(AppSettings.Read("ui", "use_window_mini_mode")); } catch { }
                if (AppValues.miniMode)
                {
                    label21.Visible = true;
                    label22.Visible = true;
                    vitnX2_Panel4.Visible = true;
                    Size = new Size(210, 229);
                }
                else
                {
                    Size = new Size(757, 362);
                    label21.Visible = false;
                    label22.Visible = false;
                    vitnX2_Panel4.Visible = false;
                }
                try { VitNX3.Functions.WindowAndControls.Controls.SetNativeThemeForControls(eventsLog.Handle); } catch { }
                try { VitNX3.Functions.WindowAndControls.Controls.SetNativeThemeForControls(pluginsManagerList.Handle); } catch { }
            }
            try
            {
                if (!Directory.Exists($@"{Application.StartupPath}\data\plugins"))
                    Directory.CreateDirectory($@"{Application.StartupPath}\data\plugins");
                if (!SysFile.Exists($@"{Application.StartupPath}\data\plugins\installed.cfg"))
                    SysFile.WriteAllText($@"{Application.StartupPath}\data\plugins\installed.cfg", "");
            }
            catch (Exception ex)
            {
                if (AppValues.serviceMode)
                    Console.WriteLine($"Plugin list error! {ex.Message}");
                else
                    VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Ошибка списка плагинов:\n{ex.Message}", "Невозможно загрузить плагины", MessageBoxButtons.OK, MessageBoxIcon.Error);
                log.Write($"Ошибка списка плагинов: {ex.Message}");
            }
        }

        public void AddEvent(string text,
            bool onlyLog = false)
        {
            text = text.Trim();
            if (!onlyLog)
            {
                if (!text.Contains("Object reference not set to an instance of an object."))
                {
                    eventsLog.Items.Add(text);
                    log.Write(text);
                }
            }
            else
                log.Write($"[BOT] {text}");
        }

        public static bool IsChanged = false;

        private void Home_Click(object sender, EventArgs e)
        {
            // TODO Add animations for burger line
            //try { Animations.Stop(); } catch { }
            //Timer timer = new Timer();
            //Animations.Tick += new EventHandler((__, _) =>
            //{
            //    if (selectedMenu.Location != new Point(88, 66))
            //        selectedMenu.Location = new Point(selectedMenu.Location.X, selectedMenu.Location.Y — 1);
            //    if (selectedMenu.Location == new Point(88, 66))
            //        Animations.Stop();
            //});
            //Animations.Start();
            if (IsChanged)
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Вы забыли применить настройки!",
                     "Невозможно применить настройки!",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
            else
            {
                selectedMenu.Location = new Point(11, 166);
                burgerControl.SelectedIndex = 0;
            }
        }

        public bool firstScCheck = false;

        private void Settings_Click(object sender, EventArgs e)
        {
            if (botPowerControl.Checked)
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Настройки недоступны, пока включён бот!",
                     "Невозможно открыть настройки!",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
            else
            {
                selectedMenu.Location = new Point(11, 200);
                if (Directory.Exists("service"))
                {
                    firstScCheck = true;
                    vitnX2_ToogleButton2.Checked = true;
                }
                burgerControl.SelectedIndex = 1;
            }
        }

        private void Plugins_Click(object sender, EventArgs e)
        {
            if (IsChanged)
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Вы забыли применить настройки!",
                     "Невозможно применить настройки!",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
            else
            {
                selectedMenu.Location = new Point(11, 235);
                burgerControl.SelectedIndex = 2;
            }
        }

        private void Help_Click(object sender, EventArgs e)
        {
            if (Processes.OpenLink("https://teletype.in/@zalexanninev15/Shark-Remote-Documentation") == false)
                Processes.Open("https://teletype.in/@zalexanninev15/Shark-Remote-Documentation");
        }

        private async void botPower_CheckedChangedAsync(object sender, EventArgs e)
        {
            botName.Texts = "";
            botUsername.Texts = "";
            botId.Texts = "";
            botPowerControlStatus.Texts = "";
            GetSettings(false);
            var botClient = new TelegramBotClient(AppValues.botToken);
            using (var cts = new CancellationTokenSource())
            {
                var receiverOptions = new ReceiverOptions
                {
                    AllowedUpdates = Array.Empty<UpdateType>()
                };
                if (AppValues.botToken != "")
                {
                    if (botPowerControl.Checked)
                    {
                        botPowerControl.Enabled = false;
                        statusBot.Text = "Подкл.";
                        try
                        {
                            botClient.StartReceiving(
                            HandleUpdateAsync,
                            HandlePollingErrorAsync,
                            receiverOptions,
                            cts.Token);
                            statusBot.Text = "Вкл.";
                            botPowerControl.Enabled = true;
                            var botInfo = botClient.GetMeAsync().Result;
                            botName.Texts = botInfo.FirstName;
                            botUsername.Texts = botInfo.Username;
                            botId.Texts = botInfo.Id.ToString();
                            AddEvent($"Имя бота = {botName.Texts}", true);
                            AddEvent($"Username бота = {botUsername.Texts}", true);
                            AddEvent($"ID бота = {botId.Texts}", true);
                            botPowerControlStatus.ForeColor = Color.FromArgb(153, 230, 153);
                            botPowerControlStatus.Texts = "Активирован";
                            AddEvent($"Ожидаю ввода команд...");
                        }
                        catch (Exception ex)
                        {
                            try { cts.Cancel(); } catch { }
                            botPowerControlStatus.ForeColor = Color.FromArgb(198, 87, 96);
                            botPowerControlStatus.Texts = "Ошибка";
                            botPowerControl.Enabled = true;
                            statusBot.Text = "Откл.";
                            AddEvent(ex.Message, true);
                            botPowerControl.Checked = false;
                        }
                    }
                    else
                    {
                        try { cts.Cancel(); } catch { }
                        botPowerControlStatus.ForeColor = Color.FromArgb(215, 99, 90);
                        botPowerControlStatus.Texts = "Деактивирован";
                        statusBot.Text = "Откл.";
                        eventsLog.Items.Clear();
                        AddEvent("Ожидаю включения...");
                    }
                }
                else
                {
                    try { cts.Cancel(); } catch { }
                    botPowerControlStatus.ForeColor = Color.FromArgb(215, 99, 90);
                    botPowerControlStatus.Texts = "Деактивирован";
                    statusBot.Text = "Откл.";
                    AppValues.botToken = "";
                    eventsLog.Items.Clear();
                    AddEvent("Ожидаю включения...");
                }
            }
        }

        public async Task HandleUpdateAsync(ITelegramBotClient botClient,
            Update update,
            CancellationToken cancellationToken)
        {
            try
            {
                if (botPowerControl.Checked)
                {
                    if (update.Message is not { } message)
                        return;

                    var chatId = update.Message.Chat.Id;
                    var username = update.Message.Chat.Username;
                    //if (TelegramBot.IsMyUser(username))
                    if (AppSettings.Read("bot", "username") == username)
                    {
                        if (update.Message.Type == MessageType.Text)
                        {
                            await HandleMessageAsync(botClient,
                                update.Message,
                                cancellationToken);
                            return;
                        }
                        if (update.Message.Type == MessageType.Document)
                        {
                            await HandleFileAsync(botClient,
                                update.Message,
                                cancellationToken);
                            return;
                        }
                        if (update.Message.Type == MessageType.Audio)
                        {
                            await HandleAudioAsync(botClient,
                                update.Message,
                                cancellationToken);
                            return;
                        }
                    }
                    else
                    {
                        await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🔒 <b>Access to the bot is only allowed to users whose <b>username</b> is entered in the list of users in the application settings!</b>",
                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                        BeginInvoke(new Action(() => { AddEvent($"Пользователь @{username} попытался получить доступ к боту!"); }));
                    }
                }
            }
            catch (Exception ex)
            {
                AddEvent($"Ошибка: {ex.Message}");
            }
        }

        public async Task HandleFileAsync(ITelegramBotClient botClient,
            Telegram.Bot.Types.Message message,
            CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var username = message.Chat.Username;
            var messageDocument = message.Document;
            var messageCaption = message.Caption;
            if (!Directory.Exists("saved"))
                Directory.CreateDirectory("saved");
            if (!Directory.Exists($@"{Application.StartupPath}\data\cache"))
                Directory.CreateDirectory(@$"{Application.StartupPath}\data\cache");
            BeginInvoke(new Action(() =>
            {
                AddEvent($"Получаю файл '{messageDocument.FileName}' от @{username}...");
            }));
            try
            {
                var documentMessageTypeFileId = messageDocument.FileId;
                var documentMessageTypeFileInfo = await botClient.GetFileAsync(documentMessageTypeFileId);
                var documentMessageTypeFilePath = documentMessageTypeFileInfo.FilePath;
                var documentMessageTypeFileSize = documentMessageTypeFileInfo.FileSize;
                var documentMessageTypeFileName = messageDocument.FileName;
                if (messageCaption == "" || messageCaption == null)
                {
                    await botClient.SendChatActionAsync(chatId: chatId,
                    ChatAction.Typing);
                    var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🟡 Сохраняю файл...");
                    string destinationFilePath = @$"{Application.StartupPath}\saved\{documentMessageTypeFileName}";
                    try
                    {
                        if (SysFile.Exists(destinationFilePath))
                            File.DeleteForever(destinationFilePath);
                    }
                    catch { }
                    await DownloadContentManager(botClient,
                        message,
                        destinationFilePath,
                        ContentType.File);
                    await botClient.EditMessageTextAsync(chatId: chatId,
                            messageId: processMessage.MessageId,
                            text: "😋 <b>Файл сохранён в папку <code>saved</code>!</b>",
                            parseMode: ParseMode.Html);
                }
                else
                {
                    if (Convert.ToString(messageCaption).ToLower() == "desktop")
                    {
                        await botClient.SendChatActionAsync(chatId: chatId,
                        ChatAction.Typing);
                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🟡 Сохраняю файл и применяю как фон рабочего стола...");
                        string destinationFilePath = @$"{Application.StartupPath}\data\cache\desktop_wallpaper.image";
                        try
                        {
                            if (SysFile.Exists(destinationFilePath))
                                File.DeleteForever(destinationFilePath);
                        }
                        catch { }
                        await DownloadContentManager(botClient,
                            message,
                            destinationFilePath,
                            ContentType.File);
                        await botClient.EditMessageTextAsync(chatId: chatId,
                            messageId: processMessage.MessageId,
                            text: "😋 <b>Файл сохранён в кэше и применён как фоновое изображение для Рабочего стола!</b>",
                            parseMode: ParseMode.Html);
                        DesktopWallpaper.Set(destinationFilePath);
                    }
                    if (Convert.ToString(messageCaption).ToLower() == "tprint")
                    {
                        await botClient.SendChatActionAsync(chatId: chatId, ChatAction.Typing);
                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🟡 Печатаю...");
                        await botClient.SendChatActionAsync(chatId: chatId,
                            ChatAction.Typing);
                        string destinationFilePath = @$"{Application.StartupPath}\data\cache\{File.NameGenerator("print", "text")}";
                        try
                        {
                            if (SysFile.Exists(destinationFilePath))
                                File.DeleteForever(destinationFilePath);
                        }
                        catch { }
                        await DownloadContentManager(botClient,
                            message,
                            destinationFilePath,
                            ContentType.File);
                        tfileStr = SysFile.ReadAllText(destinationFilePath);

                        PrintDocument printDocument = new PrintDocument();
                        printDocument.PrintPage += PrintPageHandler;
                        PrintDialog printDialog = new PrintDialog();
                        printDialog.Document = printDocument;
                        printDialog.Document.Print();
                        await botClient.EditMessageTextAsync(chatId: chatId,
                            messageId: processMessage.MessageId,
                            text: "😋 <b>Файл сохранён в кэше и распечатан!</b>",
                            parseMode: ParseMode.Html);
                    }
                }
                BeginInvoke(new Action(() =>
                {
                    AddEvent($"Получен файл '{messageDocument.FileName}' от @{username}");
                }));
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    AddEvent($"Файл '{messageDocument.FileName}' от @{username} недоступен!");
                    if (ex.Message.Contains("Bad Request: file is too big"))
                        AddEvent($"[TelegramBotAPI] Размер файла слишком большой!");
                }));
                AddEvent(ex.Message, true);
            }
        }

        public async Task HandleAudioAsync(ITelegramBotClient botClient,
            Telegram.Bot.Types.Message message,
            CancellationToken cancellationToken)
        {
            var chatId = message.Chat.Id;
            var username = message.Chat.Username;
            var messageAudio = message.Audio;
            var messageCaption = message.Caption;
            if (!Directory.Exists("saved"))
                Directory.CreateDirectory("saved");
            if (!Directory.Exists($@"{Application.StartupPath}\data\cache"))
                Directory.CreateDirectory(@$"{Application.StartupPath}\data\cache");
            BeginInvoke(new Action(() =>
            {
                AddEvent($"Получаю аудио '{messageAudio.FileName}' от @{username}...");
            }));
            try
            {
                var audioMessageTypeFileId = messageAudio.FileId;
                var audioMessageTypeFileInfo = await botClient.GetFileAsync(audioMessageTypeFileId);
                var audioMessageTypeFilePath = audioMessageTypeFileInfo.FilePath;
                var audioMessageTypeFileSize = audioMessageTypeFileInfo.FileSize;
                var audioMessageTypeFileName = messageAudio.FileName;
                string destinationFilePath = @$"{Application.StartupPath}\saved\{audioMessageTypeFileName}";
                if (messageCaption == "" || messageCaption == null)
                {
                    await botClient.SendChatActionAsync(chatId: chatId,
                    ChatAction.Typing);
                    var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🟡 Сохраняю аудио...");
                    try
                    {
                        if (SysFile.Exists(destinationFilePath))
                            File.DeleteForever(destinationFilePath);
                    }
                    catch { }
                    await DownloadContentManager(botClient,
                        message,
                        destinationFilePath,
                        ContentType.Audio);
                    await botClient.EditMessageTextAsync(chatId: chatId,
                            messageId: processMessage.MessageId,
                            text: "😋 <b>Аудио сохранено в папку <code>saved</code>!</b>",
                            parseMode: ParseMode.Html);
                }
                else
                {
                    if (Convert.ToString(messageCaption).ToLower() == "play")
                    {
                        await botClient.SendChatActionAsync(chatId: chatId,
                        ChatAction.Typing);
                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🟡 Сохраняю аудио и воспроизвожу...");
                        try
                        {
                            if (SysFile.Exists(destinationFilePath))
                                File.DeleteForever(destinationFilePath);
                        }
                        catch { }
                        await DownloadContentManager(botClient,
                            message,
                            destinationFilePath,
                            ContentType.Audio);
                        Processes.Open(destinationFilePath);
                        await botClient.EditMessageTextAsync(chatId: chatId,
                            messageId: processMessage.MessageId,
                            text: "😋 <b>Аудио сохранено в папку <code>saved</code> и воспроизведено!</b>",
                            parseMode: ParseMode.Html);
                        DesktopWallpaper.Set(destinationFilePath);
                    }
                }
                BeginInvoke(new Action(() =>
                {
                    AddEvent($"Получено аудио '{messageAudio.FileName}' от @{username}");
                }));
            }
            catch (Exception ex)
            {
                BeginInvoke(new Action(() =>
                {
                    AddEvent($"Аудио '{messageAudio.FileName}' от @{username} недоступно!");
                    if (ex.Message.Contains("Bad Request: audio is too big"))
                        AddEvent($"[TelegramBotAPI] Размер аудио слишком большой!");
                }));
                AddEvent(ex.Message, true);
            }
        }

        public async Task HandleMessageAsync(ITelegramBotClient botClient,
        Telegram.Bot.Types.Message message,
        CancellationToken cancellationToken)
        {
            try
            {
                string[] command = new string[2];
                var chatId = message.Chat.Id;
                var username = message.Chat.Username;
                var messageText = message.Text;
                command[0] = TelegramBot.GetCommand(messageText);
                command[1] = TelegramBot.GetArguments(messageText, command[0]);
                var _command = TelegramBot.IsMyCommand(command[0]);
                if (_command == TelegramBot.BotCommandType.NATIVE)
                {
                    if (command[0] != command[1].Replace(@"/", ""))
                        BeginInvoke(new Action(() => { AddEvent($"Принята команда '{command[0]}' с аргументом '{command[1]}' от @{username}"); }));
                    else BeginInvoke(new Action(() =>
                    {
                        if (command[1].Contains(@"/"))
                            AddEvent($"Принята команда '{command[0]}' от @{username}");
                    }));
                    switch (command[0])
                    {
                        case "📩 Отправка и сохранение":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                text: $"<b>📩 Отправка и сохранение</b>\n\n" +
                                "💾 Сохраните файл (документ) на PC просто отправив его боту\n" +
                                "* отправка без подписи - скачать файл в папку <code>saved</code>\n" +
                                "* подпись <code>desktop</code> - установить файл (необходимо отправить картинку как документ) как фоновое изображение для Рабочего стола\n" +
                                "* подпись <code>tprint</code> - распечатать текстовый файл\n\n" +
                                "🎧 Также можно сохранять аудио\n" +
                                "* отправка без подписи - скачать аудио в папку <code>saved</code>\n" +
                                "* подпись <code>play</code> - скачать аудио в папку <code>saved</code> и воспроизвести",
                                 parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "🗃 Файлы и папки":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                text: $"<b>🗃 Файлы и папки</b>" +
                                "\n/ls [папка] - содержимое папки" +
                                 "\n/ls0 [папка] - запись содержимого папки в текстовый файл" +
                                "\n/md [папка] - создать папку" +
                                "\n/clean - очистить Корзину" +
                                "\n/send [объект] - загрузить файл/папку в Telegram" +
                                "\n/tprint [файл] - печать текстового файла (имеются настройки для Shark Remote)" +
                                "\n/cat [файл] - показ текстового содержимого файла" +
                                "\n/file [файл] - информация о файле" +
                                "\n/dir [папка] - информация о папке" +
                                "\n/del [файл] - удалить файл в Корзину" +
                                "\n/rd [папка] - удалить папку в Корзину",
                                 parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "🕹 Управление":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                text: $"<b>🕹 Управление</b>" +
                                "\n/screen - сделать скриншот" +
                                "\n/wh - переключить режим показа окон" +
                                "\n/power [действие] - управление питанием:" +
                                "\n\t<code>logoff</code> - выход из системы" +
                                "\n\t<code>off</code> - выключение" +
                                "\n\t<code>reboot</code> - перезагрузка" +
                                "\n\t<code>lock</code> - блокировка" +
                                "\n\t<code>mon</code> - включение монитора" +
                                "\n\t<code>monoff</code> - выключение монитора" +
                                "\n/vset [0-100] - задать уровень громкости звука в %" +
                                "\n/vget - получить уровень громкости звука в %" +
                                 "\n/get - получить текст из буфера обмена" +
                                 "\n/set [текст] - задать текст для буфера обмена" +
                                 "\n/apps - установленные приложения" +
                                 "\n/info - информация о PC" +
                                 "\n/battery - информация о батарее" +
                                 "\n/msg [тип сообщения] [текст] - отправка сообщения:" +
                                 "\n\t<code>{n}</code> - обычное сообщение" +
                                 "\n\t<code>{i}</code> - информация" +
                                 "\n\t<code>{w}</code> - предупреждение" +
                                 "\n\t<code>{e}</code> - ошибка",
                                 parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "🌐 Сеть":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                text: $"<b>🌐 Сеть</b>" +
                                "\n/geo - получение местоположения" +
                                "\n/net - краткая сетевая информация" +
                                "\n/ping [сайт/IP адрес] - пропинговка сайта или IP-адреса",
                                 parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "📜 Задачи":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                text: $"<b>📜 Задачи</b>" +
                                 "\n/run [объект] - запуск файла/папки/приложения" +
                                "\n/run [файл приложения]|[аргументы] - запуск приложения с аргументом(ами)" +
                                 "\n/tasks - запущенные процессы" +
                                "\n/kill [файл приложения (можно без EXE)] - завершить все процессы приложения" +
                                 "\n/killcmd - завершить все процессы Cmd" +
                                 "\n/killps - завершить все процессы PowerShell" +
                                "\n/sc [действие] [название службы] - управление службами:" +
                                 "\n\t<code>get</code> - получить список служб (название службы не требуется)" +
                                 "\n\t<code>start</code> - запустить службу" +
                                 "\n\t<code>stop</code> - остановить службу" +
                                 "\n\t<code>restart</code> - перезапустить службу",
                                 parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "🤏 Другое":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "<b>🤏 Другое</b>" +
                                    "\n/start - запуск бота" +
                                    "\n/bot - информация о боте",
                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "🪴 Пользовательские":
                            {
                                bool havePlg = false;
                                string iPlg = "<b>📦 Плагины</b>";
                                string[] listCFG = SysFile.ReadAllLines($@"{Application.StartupPath}\data\plugins\installed.cfg");
                                foreach (string item in listCFG)
                                {
                                    if (item != "")
                                    {
                                        if (!item.StartsWith('#'))
                                        {
                                            string args = "";
                                            int agumentsPlugin = Convert.ToInt32(AppSettings.Read("", "arguments_count", AppSettings.TomlTypeRead.OnlyOneKey, @$"plugins\{item.Split(", ")[0].Remove(0, 4)}\main.manifest"));
                                            if (agumentsPlugin > 0 && agumentsPlugin <= 4)
                                                if (agumentsPlugin == 1)
                                                    args += "[аргумент 1] ";
                                                else if (agumentsPlugin == 2)
                                                    args += "[аргумент 1]|[аргумент 2] ";
                                                else if (agumentsPlugin == 3)
                                                    args += "[аргумент 1]|[аргумент 2]|[аргумент 3] ";
                                                else if (agumentsPlugin == 4)
                                                    args += "[аргумент 1]|[аргумент 2]|[аргумент 3]|[аргумент 4] ";
                                            iPlg += "\n" + item.Split(", ")[3].Remove(0, 8) + $" {args}- " + item.Split(", ")[0].Remove(0, 4) + "\n";
                                            havePlg = true;
                                        }
                                    }
                                }
                                if (!havePlg)
                                    iPlg += "\nНе установлены!\n";
                                string[] listTXT = SysFile.ReadAllLines($@"{Application.StartupPath}\data\settings\variables.txt");
                                string varList = "\n<b>🪴 Пользовательские переменные</b>";
                                foreach (string item in listTXT)
                                {
                                    if (item != "")
                                        varList += $"\n<code>{item.Split('=')[0]}</code>={item.Split("=")[1]}";
                                }
                                await botClient.SendTextMessageAsync(chatId: chatId, text: iPlg + varList,
                                parseMode: ParseMode.Html, cancellationToken: cancellationToken,
                                replyMarkup: AppValues.keyboardMoreInfoQAVA, disableWebPagePreview: true);
                                break;
                            }
                        case "start":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId, text: "Привет!\n\n" +
                                    "<b>Я твой персональный бот для удалённого управления PC под руководством приложения Shark Remote</b>\n" +
                                    "Больше информации о боте смотри тут /bot" +
                                    "\n\n👇 <b>С помощью меню ниже ты сможешь приступить к управлению своим PC</b>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: AppValues.GetKeyboard());
                                break;
                            }
                        case "screen":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Делаю скриншот...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.UploadPhoto);
                                string fileName = string.Format("Скриншот_{0}_{1}.png",
                                    DateTime.Now.ToString("yyyyMMddHHmmssfff"),
                                    Guid.NewGuid());
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.UploadDocument);
                                InputOnlineFile inputOnlineFile = new InputOnlineFile(VitNX3.Functions.Information.Monitor.CaptureScreenToMemoryStream(),
                                    fileName);
                                await botClient.DeleteMessageAsync(chatId: chatId,
                                    messageId: processMessage.MessageId);
                                await botClient.SendDocumentAsync(chatId: chatId,
                                    inputOnlineFile,
                                    caption: "🖥 <b>Скриншот загружен!</b>",
                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;
                            }
                        case "geo":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю местоположение...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.FindLocation);
                                try
                                {
                                    string[] geo = await TelegramBot.GetGeoAsync(ip);
                                    var keyboardGeo = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithUrl("ℹ Получить больше информации",
                                            $"https://ipgeolocation.io/ip-location/{ip}"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithUrl("🗺 Показать на карте",
                                            $"https://www.google.com/maps/search/?api=1&query={geo[1]},{geo[2]}"),
                                        }
                                    });
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: $"📍 <b>Местоположение</b>\n{geo[0]}",
                                        parseMode: ParseMode.Html,
                                        replyMarkup: keyboardGeo);
                                    try
                                    {
                                        Convert.ToSingle(geo[1], new CultureInfo("en-US"));
                                        float la_map;
                                        float.TryParse(geo[1], NumberStyles.Any, new CultureInfo("en-US"),
                                            out la_map);
                                        Convert.ToSingle(geo[2], new CultureInfo("en-US"));
                                        float lo_map; float.TryParse(geo[2], NumberStyles.Any,
                                            new CultureInfo("en-US"), out lo_map);
                                        await botClient.SendVenueAsync(chatId: chatId,
                                            latitude: la_map,
                                            longitude: lo_map,
                                            title: "📍 Местоположение компьютера",
                                            address: "Подробнее смотрите выше");
                                    }
                                    catch { }
                                }
                                catch
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Сервис не отвечает!</b>" +
                                        "\nПовторите попытку позже.",
                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "net":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю сетевую информацию...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                try
                                {
                                    var keyboard = new InlineKeyboardMarkup(new[]
                                    {
                                            new[]
                                            {
                                                InlineKeyboardButton.WithUrl("ℹ Получить больше информации",
                                                $"https://2ip.ru/whois/?ip={ip}"),
                                            }
                                        });
                                    try
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                                 text: $"🌐 <b>Сетевая информация</b>" +
                                                 $"\nИмя хоста: <code>{Dns.GetHostName()}</code>" +
                                                 $"\nПубличный IP-адрес: <code>{ip}</code>" +
                                                 $"\nЛокальный IP-адрес (IPv6): <code>{Dns.GetHostByName(Dns.GetHostName()).AddressList[0]}</code>" +
                                                 $"\nЛокальный IP-адрес (IPv4): <code>{Dns.GetHostByName(Dns.GetHostName()).AddressList[1]}</code>",
                                                 parseMode: ParseMode.Html,
                                                 replyMarkup: keyboard);
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                                 text: $"🌐 <b>Сетевая информация</b>" +
                                                 $"Имя хоста: <code>{Dns.GetHostName()}</code>" +
                                                 $"\nПубличный IP-адрес: <code>{ip}</code>" +
                                                 $"\nЛокальный IP-адрес: <code>{Dns.GetHostByName(Dns.GetHostName()).AddressList[0]}</code>",
                                                 parseMode: ParseMode.Html,
                                                 replyMarkup: keyboard);
                                    }
                                }
                                catch
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Сервис не отвечает!</b>" +
                                        "\nПовторите попытку позже.",
                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "ls":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Формирую вывод содержимого папки...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                if (command[1] != "")
                                {
                                    if (Directory.Exists(command[1]))
                                    {
                                        if (!command[1].EndsWith('\\')) command[1] = $"{command[1]}\\";
                                        try
                                        {
                                            List<string> ls = FileSystem.ReturnRecursFList(command[1]);
                                            string message1 = "";
                                            foreach (string fname in ls)
                                                message1 += $"{fname}\n";
                                            bool IsDisk = false;
                                            foreach (Match m in Regex.Matches(command[1],
                                                @"[A-Z]:\\"))
                                            {
                                                if (command[1].EndsWith(m.Value))
                                                    IsDisk = true;
                                            }
                                            if (IsDisk)
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId,
                                                    text: "📁 <b>Содержимое корня диска</b>" +
                                                $"\n<code>{message1.Trim('\n')}</code>",
                                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                            else
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId,
                                                    text: "📁 <b>Содержимое папки</b>" +
                                            $"\n<code>{message1.Trim('\n')}</code>",
                                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                        catch
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Папка пустая или элементов в папке слишком много!</b>" +
                                            "\nЕсли вы уверены, что папка не пустая, то воспользуйтесь командой /ls0, " +
                                            "чтобы получить элементы расположенные в папке в виде текстового файла.",
                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                    }
                                    else await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Данного пути не существует!</b>",
                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к папке!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "ls0":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Формирую вывод содержимого папки...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.UploadDocument);
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                if (command[1] != "")
                                {
                                    if (Directory.Exists(command[1]))
                                    {
                                        if (!command[1].EndsWith('\\')) command[1] = $"{command[1]}\\";
                                        try
                                        {
                                            List<string> ls = Helpers.FileSystem.ReturnRecursFList(command[1]);
                                            string message1 = "";
                                            foreach (string fname in ls)
                                                message1 += $"{fname}\n";
                                            bool IsDisk = false;
                                            foreach (Match m in Regex.Matches(command[1],
                                                @"[A-Z]:\\"))
                                            {
                                                if (command[1].EndsWith(m.Value))
                                                    IsDisk = true;
                                            }
                                            if (IsDisk)
                                            {
                                                await botClient.DeleteMessageAsync(chatId: chatId,
                                                messageId: processMessage.MessageId);
                                                SysFile.WriteAllText($@"{tempPath}\Папка.txt", $"Содержимое корня диска {command[1].Remove(2, 1)}\n{message1}");
                                                using (FileStream stream = SysFile.OpenRead($@"{tempPath}\Папка.txt"))
                                                {
                                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream,
                                                        "Папка.txt");
                                                    await botClient.SendDocumentAsync(chatId,
                                                        inputOnlineFile,
                                                        caption: "🖴 <b>Список содержимого корня диска загружен!</b>",
                                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                                }
                                                try { File.DeleteForever($@"{tempPath}\Папка.txt"); } catch { }
                                            }
                                            else
                                            {
                                                await botClient.DeleteMessageAsync(chatId: chatId,
                                                messageId: processMessage.MessageId);
                                                SysFile.WriteAllText($@"{tempPath}\Папка.txt", $"Содержимое папки \"{command[1]}\":\n{message1}");
                                                using (FileStream stream = SysFile.OpenRead($@"{tempPath}\Папка.txt"))
                                                {
                                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream, "Папка.txt");
                                                    await botClient.SendDocumentAsync(chatId,
                                                        inputOnlineFile,
                                                        caption: "📁 <b>Список содержимого папки загружен!</b>",
                                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                                }
                                                try { File.DeleteForever($@"{tempPath}\Папка.txt"); } catch { }
                                            }
                                        }
                                        catch
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Папка пустая!</b>",
                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                    }
                                    else await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Данного пути не существует!</b>",
                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к папке!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "wh":
                            {
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                text: "🪟 <b>Режим показа окон переключён!</b>",
                                                parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                Win64.Keyboard.WindowsKeyboardEventsAPI(0);
                                break;
                            }
                        case "bot":
                            {
                                var keyboardBot = new InlineKeyboardMarkup(new[]
                                    {
                                        new[]
                                        {
                                            InlineKeyboardButton.WithUrl("🦈 Сайт",
                                            $"https://sharkremote.neocities.org"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithUrl("🆕 Новостной канал",
                                            $"https://t.me/NewsWiT"),
                                        },
                                        new[]
                                        {
                                            InlineKeyboardButton.WithUrl("✍️ Связаться с автором",
                                            $"https://t.me/Zalexanninev15"),
                                        },
                                    });
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: $"ℹ️ Shark Remote - приложение для создания Telegram бота для управления PC с Windows. Сейчас используется версия {Application.ProductVersion}\n\n" +
                                    "🤖 <b>Токен бота:</b>" +
                                    $"\n<code>{AppValues.botToken}</code>\n\n" +
                                    $"📁 <b>Shark Remote расположен по пути:</b>\n" +
                                    $"<code>{Application.StartupPath}</code>\n" +
                                    "\n👤 <b>Ваш UserID:</b>" +
                                    $"\n<code>{chatId}</code>",
                                    parseMode: ParseMode.Html,
                                    replyMarkup: keyboardBot);
                                break;
                            }
                        case "power":
                            {
                                if (command[1].StartsWith("logoff") || command[1].StartsWith("off")
                                    || command[1].StartsWith("reboot") || command[1].StartsWith("lock")
                                    || command[1].StartsWith("mon") || command[1].StartsWith("moff"))
                                {
                                    switch (command[1])
                                    {
                                        case "logoff":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🛑 <b>Выхожу из системы...</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                PowerControl.Computer(PowerControl.SYSTEM_POWER_CONTROL.SYSTEM_LOGOFF);
                                                break;
                                            }
                                        case "off":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🔌 <b>Выключаю компьютер...</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                PowerControl.Computer(PowerControl.SYSTEM_POWER_CONTROL.SYSTEM_SHUTDOWN);
                                                break;
                                            }
                                        case "reboot":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🔁 <b>Перезагружаю компьютер...</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                PowerControl.Computer(PowerControl.SYSTEM_POWER_CONTROL.SYSTEM_REBOOT);
                                                break;
                                            }
                                        case "lock":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🔒 <b>Блокирую систему...</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                PowerControl.Computer(PowerControl.SYSTEM_POWER_CONTROL.SYSTEM_LOCK);
                                                break;
                                            }
                                        case "mon":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🖥 <b>Включаю монитор...</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                PowerControl.Monitor(true);
                                                break;
                                            }
                                        case "moff":
                                            {
                                                await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "💤 <b>Выключаю монитор...</b>",
                                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                                PowerControl.Monitor(false);
                                                break;
                                            }
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                           "🔴 <b>Необходимо указать действие!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "vset":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Изменяю значение уровня громкости звука...");
                                if (command[1] != "" && VitNX3.Functions.Data.Text.ContainsOnlyNumbers(command[1]))
                                {
                                    try
                                    {
                                        float newVolume = float.Parse(command[1]);
                                        if (newVolume > 0) newVolume = newVolume / 100;
                                        if (newVolume >= 0 && newVolume <= 1)
                                        {
                                            VolumeControl audio = new VolumeControl();
                                            audio.Set(newVolume);
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                                messageId: processMessage.MessageId,
                                                text: "🔊 <b>Задано новое значение уровня громкости звука!</b>",
                                                parseMode: ParseMode.Html,
                                                cancellationToken: cancellationToken);
                                        }
                                        else await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Значение не может быть ниже 0 или выше 100!</b>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                    }
                                    catch (Exception ex)
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Невозможно изменить уровень громкость звука!</b>" +
                                        $"\nСообщение ошибки: <code>{ex.Message}</code>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать новое значение громкости звука! От 0 до 100</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "vget":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю текущее значение уровня громкости звука...");
                                try
                                {
                                    VolumeControl audio = new VolumeControl();
                                    float currentVolume = audio.Get();
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: $"🔊 <b>Текущее значение уровня громкости звука:</b> " +
                                        $"{currentVolume * 100}%",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                catch (Exception ex)
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Невозможно получить уровень громкости звука!</b>" +
                                    $"\nСообщение ошибки: <code>{ex.Message}</code>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "get":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю текущий текст из буфера обмена...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                try
                                {
                                    GetClipboardText GetClipboard = new GetClipboardText();
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "📋 <b>Текст в буфере обмена</b>\n<code>" + GetClipboard.GetText() + "</code>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                catch
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                messageId: processMessage.MessageId,
                                text: "🔴 <b>Невозможно получить текст из буфера обмена!</b>",
                                parseMode: ParseMode.Html,
                                cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "set":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Добавляю новый текст в буфер обмена...");
                                if (command[1] != "")
                                {
                                    try
                                    {
                                        await SetClipboardText.Run(() => Clipboard.SetText(command[1]));
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "📋 <b>В буфер обмена добавлен новый текст!</b>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                    messageId: processMessage.MessageId,
                                    text: "🔴 <b>Невозможно задать текст для буфера обмена!</b>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                           "🔴 <b>Необходимо указать текст для буфера обмена!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "md":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Создаю новую папку...");
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                if (command[1] != "")
                                {
                                    if (!Directory.Exists(command[1]))
                                    {
                                        Directory.CreateDirectory(command[1]);
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "📁 <b>Папка успешно создана!</b>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                    }
                                    else await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        text: "🔴 <b>Папка с таким именем уже сушествует!</b>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать новую папку!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "clean":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Очищаю Корзину...");
                                Import.SHEmptyRecycleBin(IntPtr.Zero,
                                    null,
                                    Enums.SHERB_RECYCLE.SHERB_NO_SOUND |
                                    Enums.SHERB_RECYCLE.SHERB_NO_CONFIRMATION);
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                messageId: processMessage.MessageId,
                                    text: "🗑 <b>Корзина очищена!</b>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);
                                break;
                            }
                        case "kill":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Завершаю процесс(ы)...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                if (command[1] != "")
                                {
                                    if (!command[1].Contains(".exe"))
                                        command[1] += ".exe";
                                    if (command[1] == "cmd.exe")
                                    {
                                        Processes.KillNative("WindowsTerminal.exe");
                                        Processes.KillNative("OpenConsole.exe");
                                        Processes.KillNative("cmd.exe");
                                    }
                                    if (command[1] == "powershell.exe")
                                    {
                                        Processes.KillNative("WindowsTerminal.exe");
                                        Processes.KillNative("OpenConsole.exe");
                                        Processes.KillNative("powershell.exe");
                                    }
                                    Processes.KillNative(command[1]);
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                        processMessage.MessageId, text: $"⛏ <b>Все процессы завершены'!</b>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать имя процесса (можно без EXE)!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "killcmd":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Завершаю процесс(ы) Cmd...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                Processes.KillNative("WindowsTerminal.exe");
                                Processes.KillNative("OpenConsole.exe");
                                Processes.KillNative("cmd.exe");
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                    processMessage.MessageId, text: "⛏ <b>Все процессы Cmd завершены!</b>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);
                                break;
                            }
                        case "killps":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                       text: "🟡 Завершаю процесс(ы) PowerShell...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                Processes.KillNative("WindowsTerminal.exe");
                                Processes.KillNative("OpenConsole.exe");
                                Processes.KillNative("powershell.exe");
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                    processMessage.MessageId, text: "⛏ <b>Все процессы PowerShell завершены!</b>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);
                                break;
                            }
                        case "send":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Создаю ZIP архив с указанным объектом...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.UploadDocument);
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                if (command[1] != "")
                                {
                                    if (Directory.Exists(command[1]))
                                    {
                                        int bakCheck;
                                        DirectoryInfo dirInfo = new DirectoryInfo(command[1]);
                                        string fileName = $"{dirInfo.Name}.zip";
                                        await botClient.DeleteMessageAsync(chatId: chatId,
                                            messageId: processMessage.MessageId);
                                        processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                                text: "🟡 Упаковываю папку...");
                                        long dirLength = Dir.GetSize(new DirectoryInfo(dirInfo.FullName));
                                        if (dirLength > 0)
                                        {
                                            try
                                            {
                                                ZipFile.CreateFromDirectory(@command[1],
                                                    $@"{tempPath}\{fileName}",
                                                    CompressionLevel.SmallestSize,
                                                    true);
                                                bakCheck = 1;
                                            }
                                            catch
                                            {
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    processMessage.MessageId,
                                                    text: "🔴 <b>Папку невозможно заархивировать!</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                                bakCheck = 0;
                                            }
                                            if (bakCheck == 1)
                                            {
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    processMessage.MessageId,
                                                    text: "🟡 Загружаю архив с папкой в бота...");
                                                try
                                                {
                                                    using (FileStream fs = SysFile.OpenRead($@"{tempPath}\{fileName}"))
                                                    {
                                                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fs,
                                                            fileName);
                                                        await botClient.SendDocumentAsync(chatId: chatId,
                                                            inputOnlineFile,
                                                            caption: "🗄 <b>Папка загружена!</b>",
                                                            parseMode: ParseMode.Html,
                                                            cancellationToken: cancellationToken);
                                                    }
                                                    await botClient.DeleteMessageAsync(chatId: chatId,
                                                        messageId: processMessage.MessageId);
                                                }
                                                catch (Exception ex)
                                                {
                                                    await botClient.DeleteMessageAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId);
                                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🔴 <b>Итоговый архив получился слишком большим (больше 50 МБ) или у компьютера нестабильное соединение с Telegram для загрузки файлов!</b>" +
                                                        $"\nСообщение ошибки: <code>{ex.Message}</code>",
                                                        parseMode: ParseMode.Html,
                                                        cancellationToken: cancellationToken);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                                messageId: processMessage.MessageId,
                                                text: "🔴 <b>Папка пустая!</b>",
                                                parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                        try { File.DeleteForever(fileName); } catch { }
                                        break;
                                    }
                                    else
                                    {
                                        if (SysFile.Exists(command[1]))
                                        {
                                            FileInfo fileInf = new FileInfo(command[1]);
                                            if ((double)fileInf.Length > 0 && (double)fileInf.Length < 52428800)
                                            {
                                                string fileName = Path.GetFileName(command[1]);
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    processMessage.MessageId,
                                                    text: "🟡 Загружаю файл в бота...");
                                                try
                                                {
                                                    using (FileStream fs = SysFile.OpenRead(command[1]))
                                                    {
                                                        InputOnlineFile inputOnlineFile = new InputOnlineFile(fs,
                                                        $"{fileName}{fileInf.Extension}");
                                                        await botClient.SendDocumentAsync(chatId: chatId,
                                                            inputOnlineFile,
                                                            caption: "🗄 <b>Файл загружен!</b>",
                                                            parseMode: ParseMode.Html,
                                                            cancellationToken: cancellationToken);
                                                    }
                                                    await botClient.DeleteMessageAsync(chatId: chatId,
                                                        messageId: processMessage.MessageId);
                                                }
                                                catch (Exception ex)
                                                {
                                                    await botClient.DeleteMessageAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId);
                                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                                    text: "🔴 <b>Файл слишком большой (более 50 МБ) или у компьютера нестабильное соединение с Telegram для загрузки файлов!</b>" +
                                                        $"\nСообщение ошибки: <code>{ex.Message}</code>",
                                                        parseMode: ParseMode.Html,
                                                        cancellationToken: cancellationToken);
                                                }
                                                try { File.DeleteForever(fileName); } catch { }
                                            }
                                            else
                                            {
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId,
                                                    text: "🔴 <b>Файл пустой или слишком большой (более 50 МБ)!</b>",
                                                    parseMode: ParseMode.Html,
                                                    cancellationToken: cancellationToken);
                                            }
                                        }
                                        else
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                                messageId: processMessage.MessageId,
                                                text: "🔴 <b>Объект не найден!</b>",
                                                parseMode: ParseMode.Html,
                                                cancellationToken: cancellationToken);
                                        }
                                        break;
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Необходимо указать объект для отправки!</b>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "apps":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Собираю список установленных приложений...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.UploadDocument);
                                await Task.Run(() =>
                                {
                                    string toFile = $"Установленные приложения:\n{VitNX3.Functions.AppsAndProcesses.Installed.GetList()}";
                                    SysFile.WriteAllText($@"{tempPath}\Приложения.txt", toFile);
                                });
                                using (FileStream stream = SysFile.OpenRead($@"{tempPath}\Приложения.txt"))
                                {
                                    await botClient.SendChatActionAsync(chatId: chatId,
                                        ChatAction.UploadDocument);
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream,
                                        "Приложения.txt");
                                    await botClient.SendDocumentAsync(chatId,
                                        inputOnlineFile,
                                        caption: "📁 <b>Подготовлен список установленных приложений!</b>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                await botClient.DeleteMessageAsync(chatId: chatId,
                                    messageId: processMessage.MessageId);
                                try { File.DeleteForever($@"{tempPath}\Приложения.txt"); } catch { }
                                break;
                            }
                        case "tasks":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю список процессов...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                string toFile = $"Все процессы:\n{Processes.GetListWithInformation()}";
                                SysFile.WriteAllText(@$"{tempPath}\Процессы.txt", toFile);
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.UploadDocument);
                                await botClient.DeleteMessageAsync(chatId: chatId,
                                messageId: processMessage.MessageId);
                                using (FileStream stream = SysFile.OpenRead(@$"{tempPath}\Процессы.txt"))
                                {
                                    await botClient.SendChatActionAsync(chatId: chatId,
                                        ChatAction.UploadDocument);
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(stream,
                                       "Процессы.txt");
                                    await botClient.SendDocumentAsync(chatId,
                                        inputOnlineFile,
                                        caption: "⏳ <b>Подготовлен список процессов!</b>",
                                        parseMode: ParseMode.Html,
                                        cancellationToken: cancellationToken);
                                }
                                try { File.DeleteForever(@$"{tempPath}\Процессы.txt"); } catch { }
                                break;
                            }
                        case "cat":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Читаю файл...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    try
                                    {
                                        string fileText = File.GetText(TelegramBot.ArgumentsAsText(command[1]));
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           "📜 <b>Содержимое файла:</b>\n" +
                                           $"<code>{fileText}</code>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            "🔴 <b>Чтение файла не может быть завершено, т.к текста слишком много для одного сообщения Telegram!</b>",
                                            parseMode: ParseMode.Html,
                                            cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к файлу!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "msg":
                            {
                                if (command[1].StartsWith("{n}") || command[1].StartsWith("{i}")
                                    || command[1].StartsWith("{e}") || command[1].StartsWith("{w}"))
                                {
                                    await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                    if (command[1].StartsWith("{n}"))
                                    {
                                        command[1] = command[1].Replace("{n} ", "");
                                        BeginInvoke(new Action(() =>
                                        {
                                            MessageBox.Show(command[1],
                                            "Сообщение",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.None,
                                            MessageBoxDefaultButton.Button1,
                                            MessageBoxOptions.DefaultDesktopOnly);
                                        }));
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                           "💬 <b>Сообщение отправлено!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                    }
                                    if (command[1].StartsWith("{i}"))
                                    {
                                        command[1] = command[1].Replace("{i} ", "");
                                        BeginInvoke(new Action(() =>
                                        {
                                            MessageBox.Show(command[1],
                                            "Информация",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Information,
                                            MessageBoxDefaultButton.Button1,
                                            MessageBoxOptions.DefaultDesktopOnly);
                                        }));
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                           "💬 <b>Сообщение с информацией отправлено!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                    }
                                    if (command[1].StartsWith("{e}"))
                                    {
                                        command[1] = command[1].Replace("{e} ", "");
                                        BeginInvoke(new Action(() =>
                                        {
                                            MessageBox.Show(command[1],
                                                "Ошибка",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error,
                                                MessageBoxDefaultButton.Button1,
                                                MessageBoxOptions.DefaultDesktopOnly);
                                        }));
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                           "💬 <b>Сообщение с ошибкой отправлено!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                    }
                                    if (command[1].StartsWith("{w}"))
                                    {
                                        command[1] = command[1].Replace("{w} ", "");
                                        BeginInvoke(new Action(() =>
                                        {
                                            MessageBox.Show(command[1],
                                            "Предупреждение",
                                            MessageBoxButtons.OK,
                                            MessageBoxIcon.Warning,
                                            MessageBoxDefaultButton.Button1,
                                            MessageBoxOptions.DefaultDesktopOnly);
                                        }));
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                           "💬 <b>Сообщение с предупреждением отправлено!</b>",
                                           parseMode: ParseMode.Html,
                                           cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                       "🔴 <b>Необходимо указать тип сообщения!</b>",
                                       parseMode: ParseMode.Html,
                                       cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "battery":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю данные о батарее...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                string battery_status = Convert.ToString(SystemInformation.PowerStatus.BatteryChargeStatus),
                                    per = Convert.ToString(SystemInformation.PowerStatus.BatteryLifePercent * 100) + "%";
                                if (battery_status == "NoSystemBattery")
                                {
                                    battery_status = "Не обнаружена";
                                    per = "Нет данных";
                                }
                                if (battery_status == "Unknown")
                                    battery_status = "Неизвестно";
                                if (battery_status == "Charging")
                                    battery_status = "Заряжается...";
                                if (battery_status == "High")
                                    battery_status = "Всё отлично";
                                if ((battery_status == "Low") || (battery_status == "0"))
                                    battery_status = "Пока нормально";
                                if (battery_status == "Critical")
                                    battery_status = "Ставь на зарядку!";
                                if (battery_status == "Low, Critical, Charging")
                                    battery_status = "Не работает";
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                    messageId: processMessage.MessageId,
                                    text: "🔋 <b>Батерея:</b>" +
                                    "\nСтатус: <code>" + battery_status + "</code>" +
                                    "\nУровень заряда: <code>" + per + "</code>",
                                    parseMode: ParseMode.Html,
                                    cancellationToken: cancellationToken);
                                break;
                            }
                        case "file":
                            {
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю информацию о файле...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    FileInfo fileInf = new FileInfo(command[1]);
                                    if (fileInf.Exists)
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "📄 <b>Файл:</b>" +
                                            "\nИмя: <code>" + fileInf.Name + "</code>" +
                                            "\nПуть: <code>" + fileInf.FullName + "</code>" +
                                            "\nДата и время создания: " + fileInf.CreationTime +
                                            "\nMD5: <code>" + File.GetMD5(command[1]) + "</code>" +
                                            "\nРазмер: " + ((double)fileInf.Length / 1048576).ToString("#.# МБ") + " (<code>" + fileInf.Length + "</code> Байт)",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    else
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           "🔴 <b>Файл не найден!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к файлу!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "dir":
                            {
                                command[1] = TelegramBot.ArgumentsAsText(command[1]);
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Получаю информацию о папке...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    DirectoryInfo dirInfo = new DirectoryInfo(command[1]);
                                    if (Directory.Exists(command[1]))
                                    {
                                        long length = Dir.GetSize(new DirectoryInfo(dirInfo.FullName));
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           text: "📁 <b>Папка:</b>" +
                                           "\nИмя: <code>" + dirInfo.Name + "</code>" +
                                           "\nПуть: <code>" + dirInfo.FullName + "</code>" +
                                           "\nДата и время создания: " + dirInfo.CreationTime +
                                           "\nРазмер: " + ((double)length / 1048576).ToString("#.# МБ") + " (<code>" + length + "</code> Байт)",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    else
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           "🔴 <b>Папка не найдена!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к папке!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "del":
                            {
                                string filepath = TelegramBot.ArgumentsAsText(command[1]);
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Удаляю файл в Корзину...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    try
                                    {
                                        if (SysFile.Exists(filepath))
                                        {
                                            BeginInvoke(new Action(() => { File.DeleteToRecycleBin(filepath); }));
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                               messageId: processMessage.MessageId,
                                               "🗑 <b>Файл удалён в Корзину!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                        else
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                               messageId: processMessage.MessageId,
                                               "🔴 <b>Файл не найден!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           "🔴 <b>Удаление файла невозможно!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к файлу!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "rd":
                            {
                                string dirpath = TelegramBot.ArgumentsAsText(command[1]);
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Удаляю папку в Корзину...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    try
                                    {
                                        if (Directory.Exists(dirpath))
                                        {
                                            BeginInvoke(new Action(() => { Dir.DeleteToRecycleBin(dirpath); }));
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                               messageId: processMessage.MessageId,
                                               "🗑 <b>Папка удалена в Корзину!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                        else
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                               messageId: processMessage.MessageId,
                                               "🔴 <b>Папка не найдена!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                           messageId: processMessage.MessageId,
                                           "🔴 <b>Удаление папки невозможно!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать путь к папке!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "run":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🟡 Создаю процесс...");
                                await botClient.SendChatActionAsync(chatId: chatId,
                                    ChatAction.Typing);
                                if (command[1] != "")
                                {
                                    try
                                    {
                                        string obj = command[1].Split('|')[0];
                                        string objData = "";
                                        try { objData = command[1].Split('|')[1]; } catch { }
                                        if (objData == "")
                                            Processes.Open(obj);
                                        else
                                            Processes.Run(obj, objData);
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                        messageId: processMessage.MessageId,
                                        $"⛏ <b>Процесс создан!</b>",
                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    catch
                                    {
                                        await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            "🔴 <b>Процесс не может быть создан, т.к. объекта для запуска не существует!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                           "🔴 <b>Необходимо указать объект запуска!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "ping":
                            {
                                if (command[1] != "")
                                {
                                    string address = "";
                                    try { address = TelegramBot.ArgumentsAsText(command[1].Split()[1].Replace("https://", "").Replace("http://", "").Replace("ftp://", "").Replace("ftps://", "")); }
                                    catch { address = TelegramBot.ArgumentsAsText(command[1].Replace("https://", "").Replace("http://", "").Replace("ftp://", "").Replace("ftps://", "")); }
                                    if (address != "/ping")
                                    {
                                        Ping ping = new Ping();
                                        PingReply pingReply = null;
                                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Пингую...");
                                        await botClient.SendChatActionAsync(chatId: chatId,
                                        ChatAction.Typing);
                                        try
                                        {
                                            pingReply = ping.Send(address);
                                            if (pingReply.Status != IPStatus.TimedOut)
                                                await botClient.EditMessageTextAsync(chatId: chatId,
                                                    messageId: processMessage.MessageId,
                                                    text: $"🌐 <b>Сервер {address}</b>\n" +
                                                    $"IP-адрес: <code>{pingReply.Address}</code>\n" +
                                                    $"Статус: {Convert.ToString(pingReply.Status).Replace("Success", "Успех")}\n" +
                                                    $"Время ответа: {pingReply.RoundtripTime} сек.\n" +
                                                    $"TTL: <code>{pingReply.Options.Ttl}</code>\n" +
                                                    $"Не фрагментируется: <code>{Convert.ToString(pingReply.Options.DontFragment).Replace("True", "Да").Replace("False", "Нет")}</code>\n" +
                                                    $"Размер буфера: <code>{pingReply.Buffer.Length}</code>",
                                                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                            else
                                            {
                                                if (Convert.ToString(pingReply.Status) == "TimedOut")
                                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                                        messageId: processMessage.MessageId,
                                                        text: $"🌐 <b>Сервер {address}</b>\n" +
                                                        $"Статус: Истекло время ожидания ответа",
                                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                                else
                                                    await botClient.EditMessageTextAsync(chatId: chatId,
                                                        messageId: processMessage.MessageId,
                                                        text: $"🌐 <b>Сервер {address}</b>\n" +
                                                        $"Статус: {pingReply.Status}",
                                                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                            }
                                        }
                                        catch
                                        {
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                                messageId: processMessage.MessageId,
                                                text: "🔴 <b>Ошибка получения данных!</b>\n" +
                                                $"Сервер: <code>{address}</code>",
                                                parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        }
                                    }
                                    else
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                                text: $"🔴 <b>Требуется сайт или IP адрес!</b>",
                                                parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                               "🔴 <b>Необходимо указать действие!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "sc":
                            {
                                if (command[1] != "")
                                {
                                    if (command[1].StartsWith("get"))
                                    {
                                        await botClient.SendChatActionAsync(chatId: chatId,
                                        ChatAction.Typing);
                                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Получаю список служб...");
                                        string std = "";
                                        using (StreamWriter sw = new StreamWriter($@"{tempPath}\Службы.txt"))
                                        {
                                            sw.WriteLine("Службы:");
                                            ManagementObjectSearcher services = new ManagementObjectSearcher("SELECT * FROM Win32_Service");
                                            ManagementObjectCollection services_collection = services.Get();
                                            foreach (ManagementObject service in services_collection)
                                            {
                                                std = Convert.ToBoolean(service["Started"]) == true ? "Да" : "Нет";
                                                sw.WriteLine($"\nСлужба: {service["Name"]}\n" +
                                                    $"Название: {service["Caption"]}\n" +
                                                    $"Описание: {service["Description"]}\n" +
                                                    $"Команда запуска: {service["PathName"]}\n" +
                                                    $"Запущена: {std}");
                                            }
                                        }
                                        await botClient.SendChatActionAsync(chatId: chatId,
                                        ChatAction.UploadDocument);
                                        await botClient.DeleteMessageAsync(chatId: chatId, processMessage.MessageId);
                                        using (FileStream fs = SysFile.OpenRead($@"{tempPath}\Службы.txt"))
                                        {
                                            InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, "Службы.txt");
                                            await botClient.SendDocumentAsync(chatId: chatId, inputOnlineFile, "🛎 <b>Подготовлен список служб!</b>");
                                        }
                                        try { File.DeleteForever($@"{tempPath}\Службы.txt"); } catch { }
                                    }
                                    if (command[1].StartsWith("start"))
                                    {
                                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Запускаю службу...");
                                        var sc = ServicesControl.Start(TelegramBot.ArgumentsAsText(command[1].Replace("start ", "")));
                                        if (sc == ServicesControl.ServiceStatus.ALREADY_RUNNING)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба уже запущена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.RUNNING)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба запущена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.UNKNOWN_ERROR)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Ошибка запуска службы!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    if (command[1].StartsWith("stop"))
                                    {
                                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Останавливаю службу...");
                                        var sc = ServicesControl.Stop(TelegramBot.ArgumentsAsText(command[1].Replace("stop ", "")));
                                        if (sc == ServicesControl.ServiceStatus.ALREADY_STOPPED)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба уже остановлена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.STOPPED)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба остановлена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.UNKNOWN_ERROR)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Ошибка остановки службы!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    if (command[1].StartsWith("restart"))
                                    {
                                        var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                        text: "🟡 Перезапускаю службу...");
                                        var sc = ServicesControl.Restart(TelegramBot.ArgumentsAsText(command[1].Replace("restart ", "")));
                                        if (sc == ServicesControl.ServiceStatus.CONFLICT_RESTARTED)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба не может быть перезапущена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.RESTARTED)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🛎 <b>Служба перезапущена!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                        if (sc == ServicesControl.ServiceStatus.UNKNOWN_ERROR)
                                            await botClient.EditMessageTextAsync(chatId: chatId,
                                            messageId: processMessage.MessageId,
                                            text: "🔴 <b>Ошибка перезапуска службы!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                           "🔴 <b>Необходимо указать действие!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "tprint":
                            {
                                if (command[1] != "")
                                {
                                    string tfilePath = TelegramBot.ArgumentsAsText(command[1]);
                                    if (SysFile.Exists(tfilePath))
                                    {
                                        tfileStr = SysFile.ReadAllText(tfilePath);
                                        await botClient.SendChatActionAsync(chatId: chatId, ChatAction.Typing);
                                        PrintDocument printDocument = new PrintDocument();
                                        printDocument.PrintPage += PrintPageHandler;
                                        PrintDialog printDialog = new PrintDialog();
                                        printDialog.Document = printDocument;
                                        printDialog.Document.Print();
                                        await botClient.SendTextMessageAsync(chatId: chatId, text: "🖨 <b>Идёт печать содержимого текстового файла!</b>",
                                            parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                    else
                                    {
                                        await botClient.SendTextMessageAsync(chatId: chatId,
                                               "🔴 <b>Текстовый файл для печати не найден!</b>",
                                               parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                    }
                                }
                                else
                                {
                                    await botClient.SendTextMessageAsync(chatId: chatId,
                                           "🔴 <b>Необходимо указать текстовый файл!</b>",
                                           parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                break;
                            }
                        case "info":
                            {
                                var processMessage = await botClient.SendTextMessageAsync(chatId: chatId,
                                       text: "🟡 Собираю информацию...");
                                await botClient.SendChatActionAsync(chatId: chatId, ChatAction.UploadDocument);
                                string monitor_data = "Невозможно опознать";
                                try
                                {
                                    SysFile.WriteAllText($@"{tempPath}\getmon.cmd", Properties.Resources.getmon);
                                    Processes.RunAW("cmd", $"/c \"{Application.StartupPath}\\data\\temp\\getmon.cmd\" >\"{Application.StartupPath}\\data\\temp\\$mon.log\"", false);
                                    monitor_data = SysFile.ReadLines($@"{tempPath}\$mon.log").Skip(2).First();
                                    File.DeleteForever($@"{tempPath}\$mon.log");
                                }
                                catch { }
                                var assemblyName = AssemblyName.GetAssemblyName(Assembly.GetExecutingAssembly().Location);
                                using (StreamWriter sw = new StreamWriter($@"{tempPath}\Компьютер.txt"))
                                {
                                    sw.WriteLine("Информация о PC:");
                                    sw.WriteLine("");
                                    sw.WriteLine("------------- Система -------------");
                                    sw.WriteLine($"Версия Windows: {VitNX3.Functions.Information.Windows.GetWindowsVersion()} (x64) - {VitNX3.Functions.Information.Windows.GetWindowsProductNameFromRegistry()}");
                                    ManagementObjectSearcher _WinOS = new ManagementObjectSearcher("SELECT * FROM Win32_OperatingSystem");
                                    ManagementObjectCollection WinOS = _WinOS.Get();
                                    foreach (ManagementObject WinOSobj in WinOS)
                                        sw.WriteLine("Серийный номер: " + WinOSobj["SerialNumber"]);
                                    sw.WriteLine($"Ключ Windows: {VitNX3.Functions.Information.Windows.GetWindowsProductKeyFromRegistry()}");
                                    sw.WriteLine($"Системная папка: {Environment.SystemDirectory}");
                                    sw.WriteLine($"Имя компьютера (хост): {Environment.MachineName}");
                                    sw.WriteLine($"Имя текущего пользователя: {SystemInformation.UserName}\n");
                                    sw.WriteLine("=====================================================================================");
                                    sw.WriteLine("------------- Материнская плата -------------");
                                    ManagementObjectSearcher searcher = new ManagementObjectSearcher("SELECT * FROM Win32_BaseBoard");
                                    ManagementObjectCollection information = searcher.Get();
                                    try
                                    {
                                        foreach (ManagementObject obj in information)
                                        {
                                            sw.WriteLine("Производитель: " + obj["Manufacturer"]);
                                            sw.WriteLine("Название: " + obj["Product"]);
                                            sw.WriteLine("Модель: " + obj["Model"]);
                                            sw.WriteLine("Серийный номер: " + obj["SerialNumber"]);
                                        }
                                    }
                                    catch { }
                                    sw.WriteLine("");
                                    sw.WriteLine("------------- BIOS -------------");
                                    ManagementObjectSearcher mysBios = new ManagementObjectSearcher("SELECT * FROM Win32_BIOS");
                                    foreach (ManagementObject myBios in mysBios.Get())
                                    {
                                        sw.WriteLine("Производитель: " + myBios["Manufacturer"]);
                                        sw.WriteLine("Название: " + myBios["Name"]);
                                        sw.WriteLine("Версия: " + myBios["Version"]);
                                        sw.WriteLine("Дата выпуска: " + Convert.ToString(myBios["ReleaseDate"]).Replace("000000.000000+000", ""));
                                        sw.WriteLine("Серийный номер: " + myBios["SerialNumber"]);
                                    }
                                    sw.WriteLine($"Ключ Windows (OEM): {VitNX3.Functions.Information.Windows.GetWindowsProductKeyFromUefi()}\n");
                                    sw.WriteLine("=====================================================================================");
                                    sw.WriteLine("------------- Процессор (ЦПУ/CPU) -------------");
                                    ManagementObjectSearcher searcher8 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Processor");
                                    foreach (ManagementObject queryObj in searcher8.Get())
                                    {
                                        sw.WriteLine("Название: {0}", queryObj["Name"]);
                                        sw.WriteLine("Сокет: {0}", queryObj["SocketDesignation"]);
                                        try { sw.WriteLine("Серийный номер: {0}", queryObj["SerialNumber"]); } catch { }
                                        sw.WriteLine("Число ядер: {0}", queryObj["NumberOfCores"]);
                                        sw.WriteLine("Число потоков: {0}", queryObj["NumberOfLogicalProcessors"]);
                                        sw.WriteLine("ID: {0}", queryObj["ProcessorId"]);
                                    }
                                    sw.WriteLine("");
                                    sw.WriteLine("------------- Оперативная память (ОЗУ/RAM) -------------");
                                    ManagementObjectSearcher searcher12 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_PhysicalMemory");
                                    string ddr = "", ff = "";
                                    foreach (ManagementObject queryObj in searcher12.Get())
                                    {
                                        ddr = Convert.ToString(queryObj["MemoryType"]);
                                        ff = Convert.ToString(queryObj["FormFactor"]);
                                        switch (ddr)
                                        {
                                            case "26":
                                                ddr = "DDR4";
                                                break;

                                            case "25":
                                                ddr = "FBD2 (серверная)";
                                                break;

                                            case "24":
                                                ddr = "DDR3";
                                                break;

                                            case "22":
                                                ddr = "DDR2 FB-DIMM (серверная)";
                                                break;

                                            case "21" or "17":
                                                ddr = "DDR2";
                                                break;

                                            case "20":
                                                ddr = "DDR";
                                                break;

                                            case "12":

                                            case "9":
                                                ddr = "RAM";
                                                break;

                                            case "8":
                                                ddr = "SRAM";
                                                break;

                                            case "7":
                                                ddr = "VRAM";
                                                break;

                                            case "0" or "1":
                                                ddr = "Невозможно определить";
                                                break;
                                        }
                                        switch (ff)
                                        {
                                            case "23":
                                                ff = "LGA";
                                                break;

                                            case "12":
                                                ff = "SO-DIMM";
                                                break;

                                            case "8":
                                                ff = "DIMM";
                                                break;

                                            case "6":
                                                ff = "Проприетарный";
                                                break;

                                            case "0" or "1":
                                                ff = "Невозможно определить";
                                                break;
                                        }
                                        sw.WriteLine("Производитель: {0}", queryObj["Manufacturer"]);
                                        sw.WriteLine("Описание: {0}", queryObj["Description"]);
                                        sw.WriteLine("Серийный номер: {0}", queryObj["SerialNumber"]);
                                        sw.WriteLine("Объём: {0} ГБ", Math.Round(Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2));
                                        sw.WriteLine("Скорость: {0} МГц", queryObj["Speed"]);
                                        sw.WriteLine($"Тип памяти: {ddr}");
                                        sw.WriteLine($"Форм-фактор: {ff}");
                                        sw.WriteLine("");
                                    }
                                    sw.WriteLine("------------- Видеокарта (GPU) -------------");
                                    string ggz = "Невозможно определить";
                                    ManagementObjectSearcher searcher11 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_VideoController");
                                    foreach (ManagementObject queryObj in searcher11.Get())
                                    {
                                        sw.WriteLine("Название: {0}", queryObj["Caption"]);
                                        if ((Convert.ToString(queryObj["AdapterRAM"]) == "536870912") || (Convert.ToString(queryObj["AdapterRAM"]) == "268435456") ||
                                            (Convert.ToString(queryObj["AdapterRAM"]) == "268435456"))
                                            sw.WriteLine("Количество памяти: {0} МБ", Math.Round(Convert.ToDouble(queryObj["AdapterRAM"]) / 1024 / 1024, 2));
                                        else
                                            sw.WriteLine("Количество видеопамяти: {0} ГБ", Math.Round(Convert.ToDouble(queryObj["AdapterRAM"]) / 1024 / 1024 / 1024, 2));
                                        sw.WriteLine("Видеопроцессор: {0}", queryObj["VideoProcessor"]);
                                        ggz = Convert.ToString(queryObj["CurrentRefreshRate"]);
                                    }
                                    sw.WriteLine("");
                                    try
                                    {
                                        sw.WriteLine("------------- Монитор -------------");
                                        ManagementObjectSearcher searcher1113 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_DesktopMonitor");
                                        var monitors = VitNX3.Functions.Information.Monitor.GetMergedFriendlyNames();
                                        foreach (var monitor in monitors)
                                        {
                                            sw.WriteLine("Модель: " + monitor);
                                            if (monitor == "")
                                            {
                                                var monitorsByMonitorIds = VitNX3.Functions.Information.Monitor.GetNamesByMonitorIds();
                                                foreach (var monitorByMonitorId in monitorsByMonitorIds)
                                                    sw.WriteLine($"Модель: {monitorByMonitorId}");
                                            }
                                            foreach (ManagementObject queryObj1132 in searcher1113.Get())
                                            {
                                                sw.WriteLine("Тип: " + queryObj1132["MonitorType"]);
                                                sw.WriteLine("ID: " + queryObj1132["PNPDeviceID"]);
                                            }
                                            sw.WriteLine($"Разрешение экрана: {Screen.PrimaryScreen.Bounds.Width}x{Screen.PrimaryScreen.Bounds.Height}");
                                            if (ggz != "Невозможно определить")
                                                sw.WriteLine($"Частота обновления экрана: {ggz} Гц");
                                            else
                                                sw.WriteLine($"Частота обновления экрана: {ggz}");
                                            if (monitor_data == "")
                                                monitor_data = "Невозможно определить";
                                            sw.WriteLine($"Год выпуска: {monitor_data}");
                                        }
                                    }
                                    catch { }
                                    sw.WriteLine("\n=====================================================================================");
                                    sw.WriteLine("------------- Диски (физические) -------------");
                                    ManagementObjectSearcher mosDisks = new ManagementObjectSearcher("SELECT * FROM Win32_DiskDrive");
                                    foreach (ManagementObject moDisk in mosDisks.Get())
                                    {
                                        sw.WriteLine("Модель: {0}", moDisk["Model"]);
                                        sw.WriteLine("Серийный номер: {0}", Convert.ToString(moDisk["SerialNumber"]).Replace(" ", ""));
                                        sw.WriteLine("ID: {0}", Convert.ToString(moDisk["DeviceID"]).Replace(@"\\.\", ""));
                                        if (Convert.ToString(moDisk["InterfaceType"]) == "SCSI")
                                            sw.WriteLine("Интерфейс подключения: SATA");
                                        else
                                        {
                                            if (Convert.ToString(moDisk["InterfaceType"]) == "IDE")
                                                sw.WriteLine("Интерфейс подключения: IDE/SATA");
                                            else
                                                sw.WriteLine("Интерфейс подключения: {0}", moDisk["InterfaceType"]);
                                        }
                                        if (Convert.ToString(moDisk["MediaType"]) == "Fixed hard disk media")
                                            sw.WriteLine("Тип: HDD/SSD");
                                        if (Convert.ToString(moDisk["MediaType"]) == "Removable Media")
                                            sw.WriteLine("Тип: Флеш-накопитель (переносное устройство)");
                                        sw.WriteLine("Размер: {0} ГБ", Math.Round(Convert.ToDouble(moDisk["Size"]) / 1024 / 1024 / 1024, 2));
                                        sw.WriteLine("Разделы: {0}", moDisk["Partitions"]);
                                        sw.WriteLine("Прошивка: {0}", moDisk["FirmwareRevision"]);
                                        sw.WriteLine("Cylinders (цилиндры, совокупность всех дорожек в заданном положении привода): {0}", moDisk["TotalCylinders"]);
                                        sw.WriteLine("Сектора: {0}", moDisk["TotalSectors"]);
                                        sw.WriteLine("Головки: {0}", moDisk["TotalHeads"]);
                                        sw.WriteLine("Всего треков: {0}\n", moDisk["TotalTracks"]);
                                    }
                                    sw.WriteLine("------------- Локальные и сетевые диски, извлекаемые диски (флешки), дисководы -------------");
                                    ManagementObjectSearcher searcher44 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_Volume");
                                    foreach (ManagementObject queryObj in searcher44.Get())
                                    {
                                        sw.WriteLine("Имя диска: {0}", queryObj["Caption"]);
                                        sw.WriteLine("Буква диска: {0}", queryObj["DriveLetter"]);
                                        string typeOfDisk = "Тип диска: ";
                                        int type = Convert.ToInt32(queryObj["DriveType"]);
                                        switch (type)
                                        {
                                            case 0:
                                                sw.WriteLine($"{typeOfDisk}Неизвестный диск");
                                                break;

                                            case 2:
                                                sw.WriteLine($"{typeOfDisk}Извлекаемый диск (флешка)");
                                                break;

                                            case 3:
                                                sw.WriteLine($"{typeOfDisk}Локальный диск");
                                                break;

                                            case 4:
                                                sw.WriteLine($"{typeOfDisk}Сетевой диск");
                                                break;

                                            case 5:
                                                sw.WriteLine($"{typeOfDisk}Дисковод");
                                                break;

                                            case 6:
                                                sw.WriteLine($"{typeOfDisk}RAM-диск");
                                                break;
                                        }
                                        sw.WriteLine("Размер: {0} ГБ", Math.Round(Convert.ToDouble(queryObj["Capacity"]) / 1024 / 1024 / 1024, 2));
                                        sw.WriteLine("Файловая система: {0}", queryObj["FileSystem"]);
                                        sw.WriteLine("Свободное место: {0} ГБ\n", Math.Round(Convert.ToDouble(queryObj["FreeSpace"]) / 1024 / 1024 / 1024, 2));
                                    }
                                    sw.WriteLine("=====================================================================================");
                                    sw.WriteLine("------------- USB устройства -------------");
                                    sw.Write(VitNX3.Functions.Information.UsbDevices.UsbToString());
                                    sw.WriteLine("=====================================================================================\n------------- Сетевые устройства -------------");
                                    ManagementObjectSearcher searcher1 = new ManagementObjectSearcher(@"root\CIMV2", "SELECT * FROM Win32_NetworkAdapterConfiguration");
                                    foreach (ManagementObject queryObj in searcher1.Get())
                                    {
                                        sw.WriteLine("Название: {0}", queryObj["Caption"]);
                                        if (queryObj["DefaultIPGateway"] == null)
                                            sw.WriteLine("IP-шлюз по умолчанию: {0}", queryObj["DefaultIPGateway"]);
                                        else
                                        {
                                            string[] arrDefaultIPGateway = (string[])queryObj["DefaultIPGateway"];
                                            foreach (string arrValue in arrDefaultIPGateway)
                                                sw.WriteLine("IP-шлюз по умолчанию: {0}", arrValue);
                                        }
                                        if (queryObj["DNSServerSearchOrder"] == null)
                                            sw.WriteLine("Порядок поиска DNS-сервера: {0}", queryObj["DNSServerSearchOrder"]);
                                        else
                                        {
                                            string[] arrDNSServerSearchOrder = (string[])queryObj["DNSServerSearchOrder"];
                                            foreach (string arrValue in arrDNSServerSearchOrder)
                                                sw.WriteLine("Порядок поиска DNS сервера: {0}", arrValue);
                                        }
                                        if (queryObj["IPAddress"] == null)
                                            sw.WriteLine("IP-адрес: {0}", queryObj["IPAddress"]);
                                        else
                                        {
                                            string[] arrIPAddress = (string[])queryObj["IPAddress"];
                                            foreach (string arrValue in arrIPAddress)
                                                sw.WriteLine("IP-адрес: {0}", arrValue);
                                        }
                                        if (queryObj["IPSubnet"] == null)
                                            sw.WriteLine("IP-подсеть: {0}", queryObj["IPSubnet"]);
                                        else
                                        {
                                            string[] arrIPSubnet = (string[])queryObj["IPSubnet"];
                                            foreach (string arrValue in arrIPSubnet)
                                                sw.WriteLine("IP-подсеть: {0}", arrValue);
                                        }
                                        sw.WriteLine("MAC-адрес: {0}", queryObj["MACAddress"]);
                                        sw.WriteLine("Сервисное название: {0}\n", queryObj["ServiceName"]);
                                    }
                                }
                                await botClient.DeleteMessageAsync(chatId: chatId,
                                    messageId: processMessage.MessageId);
                                using (FileStream fs = SysFile.OpenRead($@"{tempPath}\Компьютер.txt"))
                                {
                                    InputOnlineFile inputOnlineFile = new InputOnlineFile(fs, @"Компьютер.txt");
                                    await botClient.SendDocumentAsync(chatId,
                                         inputOnlineFile,
                                         caption: "💻 <b>Информация о PC подготовлена!</b>",
                                         parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                }
                                try { File.DeleteForever($@"{tempPath}\Компьютер.txt"); } catch { }
                                try { File.DeleteForever($@"{tempPath}\$mon.log"); } catch { }
                                try { File.DeleteForever($@"{tempPath}\getmon.cmd"); } catch { }
                            }
                            break;
                    }
                }
                else if (_command == TelegramBot.BotCommandType.PLUGIN)
                {
                    string plName = Engine.API.PluginsManager.SearchPluginWithCommand(command[0]);
                    try
                    {
                        if (command[0] != command[1].Replace(@"/", ""))
                            BeginInvoke(new Action(() => { AddEvent(command[1].Contains('|') ? $"Принята команда '{command[0]}' с аргументами '{command[1].Replace("|", "' '")}' от @{username}" : $"Принята команда '{command[0]}' с аргументом '{command[1]}' от @{username}"); }));
                        else BeginInvoke(new Action(() =>
                        {
                            if (command[1].Contains(@"/"))
                                AddEvent($"Принята команда '{command[0]}' от @{username}");
                        }));
                        var messageId = await botClient.SendTextMessageAsync(chatId: chatId,
                        text: "🔃 <b>Плагин работает...</b>",
                        parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                        int chatActionTypePlugin = Convert.ToInt32(AppSettings.Read("", "chat_action_type", AppSettings.TomlTypeRead.OnlyOneKey, @$"plugins\{plName}\main.manifest"));
                        int messageTypePlugin = Convert.ToInt32(AppSettings.Read("", "message_type", AppSettings.TomlTypeRead.OnlyOneKey, @$"plugins\{plName}\main.manifest"));
                        switch (chatActionTypePlugin)
                        {
                            case 0:
                                break;

                            case 1:
                                await botClient.SendChatActionAsync(chatId: chatId, ChatAction.Typing);
                                break;
                        }
                        switch (messageTypePlugin)
                        {
                            case 0:
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                messageId.MessageId,
                                text: Engine.API.PluginsManager.RunScriptPowerShell(plName, command[1] != "" ? command[1] : "None", command[0]));
                                break;

                            case 1:
                                await botClient.EditMessageTextAsync(chatId: chatId,
                                messageId.MessageId,
                                text: Engine.API.PluginsManager.RunScriptPowerShell(plName, command[1] != "" ? command[1] : "None", command[0]),
                                parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                                break;

                            case 2:
                                await botClient.SendTextMessageAsync(chatId: chatId,
                                    text: "🎯 <b>Плагин работает в фоне!</b>", parseMode: ParseMode.Html);
                                Task.Run(() => Engine.API.PluginsManager.RunScriptPowerShell(plName, command[1] != "" ? command[1] : "None", command[0], background_worker: true));
                                break;
                        }
                    }
                    catch (Exception ex) { AddEvent($"Ошибка плагина \"{plName}\": {ex.Message}"); }
                }
                else if (_command == TelegramBot.BotCommandType.UNKNOWN)
                {
                    await botClient.SendTextMessageAsync(chatId: chatId,
                    text: "❌ <b>Введённой команды не существует!</b>",
                    parseMode: ParseMode.Html, cancellationToken: cancellationToken);
                    BeginInvoke(new Action(() => { AddEvent($"Пользователь @{username} ввёл несуществующую команду!"); }));
                }
            }
            catch (Exception ex) { AddEvent($"Неизвестная проблема бота: {ex.Message}"); }
        }

        private Task HandlePollingErrorAsync(ITelegramBotClient botClient,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var ErrorMessage = exception switch
            {
                ApiRequestException apiRequestException
                    => $"Ошибка Telegram Bot API:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
                _ => exception.ToString()
            };
            if (!ErrorMessage.Contains("[409]") && !ErrorMessage.Contains("[401]"))
                AddEvent(ErrorMessage);
            return Task.CompletedTask;
        }

        private void windowTitle_MouseDown(object sender, MouseEventArgs e)
        {
            Import.ReleaseCapture();
            Import.PostMessage(Handle,
                Constants.WM_SYSCOMMAND,
                Constants.DOMOVE, 0);
        }

        private async void titleExit_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
            {
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                {
                    form_x = Location.X;
                    form_y = Location.Y;
                    do
                    {
                        Opacity -= 0.2;
                        await Task.Delay(1);
                    }
                    while (Opacity > 0);
                }
            }
            Processes.KillNative($"Shark Remote.exe");
        }

        private void titleExit_MouseEnter(object sender, EventArgs e)
        {
            titleExit.ForeColor = Color.Red;
            label21.ForeColor = Color.Red;
        }

        private void titleExit_MouseLeave(object sender, EventArgs e)
        {
            titleExit.ForeColor = Color.FromArgb(183, 185, 191);
            label21.ForeColor = Color.FromArgb(183, 185, 191);
        }

        private void eventsLog_DoubleClick(object sender, EventArgs e)
        {
            Clipboard.SetText(eventsLog.SelectedItem.ToString());
        }

        public void PrintPageHandler(object sender, PrintPageEventArgs e)
        {
            e.Graphics.DrawString(tfileStr, new Font(AppSettings.Read("tprint", "font"),
                Convert.ToInt32(AppSettings.Read("tprint", "size"))),
                Brushes.Black, 0, 0);
        }

        private void botPowerPanel_Click(object sender, EventArgs e)
        {
            botPowerControl.Checked = !botPowerControl.Checked;
        }

        private void label3_Click(object sender, EventArgs e)
        {
            botPowerControl.Checked = !botPowerControl.Checked;
        }

        private void label4_Click(object sender, EventArgs e)
        {
            botPowerControl.Checked = !botPowerControl.Checked;
        }

        private int form_x, form_y;

        private System.Windows.Forms.Timer t = new System.Windows.Forms.Timer();

        private void minAnimation(object sender, EventArgs e)
        {
            if (Opacity <= 0)
                Hide();
            Opacity -= .2;
        }

        private async void titleMinimize_Click(object sender, EventArgs e)
        {
            if (!Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
            {
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                {
                    form_x = Location.X;
                    form_y = Location.Y;
                    do
                    {
                        Opacity -= 0.2;
                        //Height -= 10;
                        //Width -= 10;
                        Location = new Point(Location.X - 25, Location.Y + 35);
                        await Task.Delay(1);
                    }
                    while (Opacity > 0);
                }
            }
            Hide();
            ShowInTaskbar = false;
            SharkIcon.Visible = true;
        }

        private void SharkIcon_Click(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            Show();
            if (AppValues.miniMode)
                Size = new Size(206, 231);
            else
                Size = new Size(757, 362);
            Location = new Point(form_x, form_y);
            StartPosition = FormStartPosition.CenterScreen;
            if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
                Opacity = 0.96;
            else
            {
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                {
                    Opacity = 0;
                    System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                    launch.Tick += new EventHandler((sender, e) =>
                    {
                        if ((Opacity += 0.05d) == 1)
                            launch.Stop();
                    });
                    launch.Interval = 20;
                    launch.Start();
                }
            }

            SharkIcon.Visible = false;
        }

        private void openPluginsDir_Click(object sender, EventArgs e)
        {
            Processes.Open($"\"{Application.StartupPath}data\\plugins\"");
        }

        private void plgInstallPicker_Click(object sender, EventArgs e)
        {
            if (!botPowerControl.Checked)
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();
                openFileDialog1.Filter = "Plugin installer (*.srp)|*.srp";
                openFileDialog1.InitialDirectory = Application.StartupPath;
                openFileDialog1.Title = "Укажите файл установщика плагина";
                var dr = openFileDialog1.ShowDialog();
                if (dr == DialogResult.Cancel || openFileDialog1.FileName == "")
                    return;
                if (dr == DialogResult.OK && openFileDialog1.FileName != "")
                {
                    string filename = openFileDialog1.FileName;
                    string fileName = Path.GetFileName(filename).Replace(".srp", "").Split('_')[0];
                    try
                    {
                        if (Directory.Exists($@"{Application.StartupPath}data\plugins\{fileName}"))
                            try { Dir.DeleteForever($@"{Application.StartupPath}data\plugins\{fileName}"); } catch { }
                        ZipFile.ExtractToDirectory(filename, $@"{Application.StartupPath}data\plugins\{fileName}", true);
                        string[] listPlg = SysFile.ReadAllLines($@"{Application.StartupPath}data\plugins\installed.cfg");
                        string plg = SysFile.ReadAllLines($@"{Application.StartupPath}data\plugins\{fileName}\main.ps1")[0].Remove(0, 1);
                        var lines = SysFile.ReadAllLines($@"{Application.StartupPath}data\plugins\installed.cfg").Where(line => line.Trim().Split(", ")[0].Remove(0, 4) != plg.Trim().Split(", ")[0].Remove(0, 4)).ToArray();
                        SysFile.WriteAllLines($@"{Application.StartupPath}data\plugins\installed.cfg", lines);
                        SysFile.AppendAllText($@"{Application.StartupPath}data\plugins\installed.cfg", $"{plg.Trim()}\n");
                        VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Установка плагина завершена!",
                        "Плагин установлен",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        if (Directory.Exists($@"{Application.StartupPath}data\plugins\{fileName}"))
                            try { Dir.DeleteForever($@"{Application.StartupPath}data\plugins\{fileName}"); } catch { }
                        VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Плагин не найден!\nОшибка: {ex.Message}",
                            "Ошибка установки плагина",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                    }
                }
            }
            else
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Установка плагина недоступна, пока включён бот!",
                     "Невозможно установить плагин!",
                     MessageBoxButtons.OK,
                     MessageBoxIcon.Warning);
        }

        private void plgList_Click(object sender, EventArgs e)
        {
            bool havePlg = false;
            pluginsManagerList.Items.Clear();
            string[] list = SysFile.ReadAllLines($@"{Application.StartupPath}\data\plugins\installed.cfg");
            foreach (string item in list)
            {
                if (item != "")
                {
                    if (!item.StartsWith('#'))
                    {
                        pluginsManagerList.Items.Add("Название: " + item.Split(", ")[0].Remove(0, 4) +
                            "  Версия: " + item.Split(", ")[1].Remove(0, 8) + "  Автор: " + item.Split(", ")[2].Remove(0, 7) +
                            "  Команда: " + item.Split(", ")[3].Remove(0, 8));
                        havePlg = true;
                    }
                    else
                        pluginsManagerList.Items.Clear();
                }
            }
            if (!havePlg)
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Не найдено установленных плагинов!", "Нет установленных плагинов", MessageBoxButtons.OK, MessageBoxIcon.Information);
            else
                plgManagerListPanel.Visible = true;
        }

        private void plgManagerListClose_Click(object sender, EventArgs e)
        {
            plgManagerListPanel.Visible = false;
        }

        private void plgManagerListClose_MouseEnter(object sender, EventArgs e)
        {
            plgManagerListClose.ForeColor = Color.FromArgb(0, 144, 242);
        }

        private void plgManagerListClose_MouseLeave(object sender, EventArgs e)
        {
            plgManagerListClose.ForeColor = Color.FromArgb(183, 185, 191);
        }

        private void pluginsManagerList_DoubleClick(object sender, EventArgs e)
        {
            try
            {
                plgSelectedName.Text = pluginsManagerList.SelectedItem.ToString().Split("  ")[0].Remove(0, 10);
                plgSelected.Visible = true;
            }
            catch { }
        }

        private void plgBtnDelete_Click(object sender, EventArgs e)
        {
            bool isDelete = true;
            if (!botPowerControl.Checked)
            {
                var msg = VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Желаете удалить плагин \"{plgSelectedName.Text}\"?",
                    "Запрос на удаление плагина",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question,
                    MessageBoxDefaultButton.Button1);
                if (msg == DialogResult.Yes)
                {
                    try
                    {
                        string[] delPlgs = SysFile.ReadAllLines($@"{Application.StartupPath}\data\plugins\installed.cfg");
                        Thread.Sleep(50);
                        SysFile.Create($@"{Application.StartupPath}\data\plugins\installed.cfg").Close();
                        Thread.Sleep(20);
                        using (StreamWriter writer = new StreamWriter($@"{Application.StartupPath}\data\plugins\installed.cfg"))
                        {
                            foreach (string delPlg in delPlgs)
                            {
                                if (!delPlg.StartsWith('#'))
                                {
                                    if (plgSelectedName.Text == delPlg.Split(", ")[0].Remove(0, 4))
                                    {
                                        try { Dir.DeleteForever($@"{Application.StartupPath}\data\plugins\{plgSelectedName.Text}"); }
                                        catch
                                        {
                                            isDelete = false;
                                            VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Папка с плагином не может быть удалена!",
                                                "Невозможно удалить плагин",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Error);
                                        }
                                        if (isDelete)
                                        {
                                            pluginsManagerList.Items.Remove(pluginsManagerList.SelectedItem);
                                            plgSelected.Visible = false;
                                            VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Удаление плагина \"{plgSelectedName.Text}\" завершено!",
                                                "Плагин удалён",
                                                MessageBoxButtons.OK,
                                                MessageBoxIcon.Information);
                                            if (pluginsManagerList.Items.Count <= 0)
                                            {
                                                plgManagerListPanel.Visible = false;
                                                plgSelected.Visible = false;
                                            }
                                        }
                                        else
                                            writer.WriteLine(delPlg);
                                    }
                                    else
                                        writer.WriteLine(delPlg);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Ошибка удаления плагина!\n{ex.Message}",
                        "Ошибка",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    }
                }
            }
            else
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Удаление плагина недоступно, пока включён бот!",
                    "Невозможно удалить плагин!",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
        }

        private void plgSelectedExit_Click(object sender, EventArgs e)
        {
            plgSelected.Visible = false;
        }

        private void plgSelectedExit_MouseEnter(object sender, EventArgs e)
        {
            plgSelectedExit.ForeColor = Color.FromArgb(0, 144, 242);
        }

        private void plgSelectedExit_MouseLeave(object sender, EventArgs e)
        {
            plgSelectedExit.ForeColor = Color.FromArgb(183, 185, 191);
        }

        private void plgBtnScript_Click(object sender, EventArgs e)
        {
            File.ShowInExplorer($"\"{Application.StartupPath}data\\plugins\\{plgSelectedName.Text}\\main.ps1\"");
        }

        private void plgBtnDir_Click(object sender, EventArgs e)
        {
            Dir.ShowInExplorer($"{Application.StartupPath}data\\plugins\\{plgSelectedName.Text}");
        }

        private void label18_MouseEnter(object sender, EventArgs e)
        {
            label18.ForeColor = Color.FromArgb(0, 144, 242);
        }

        private void label18_MouseLeave(object sender, EventArgs e)
        {
            label18.ForeColor = Color.FromArgb(183, 185, 191);
        }

        private void titleLabel_Click(object sender, EventArgs e)
        {
            versionPanel.Visible = !versionPanel.Visible;
        }

        private void label18_Click(object sender, EventArgs e)
        {
            versionPanel.Visible = false;
        }

        private void vitnX2_Button2_Click(object sender, EventArgs e)
        {
            File.ShowInExplorer($"{Application.StartupPath}data\\shark_remote.log");
        }

        private void vitnX2_PictureBox1_Click(object sender, EventArgs e)
        {
            if (Processes.OpenLink("https://t.me/s/NewsWiT") == false)
                Processes.Open("https://t.me/s/NewsWiT");
        }

        private void vitnX2_PictureBox2_Click(object sender, EventArgs e)
        {
            if (Processes.OpenLink("https://sharkremote.neocities.org") == false)
                Processes.Open("https://sharkremote.neocities.org");
        }

        private void vitnX2_PictureBox3_Click(object sender, EventArgs e)
        {
            if (Processes.OpenLink("https://t.me/Zalexanninev15") == false)
                Processes.Open("https://t.me/Zalexanninev15");
        }

        private void titleLabel_MouseEnter(object sender, EventArgs e)
        {
            if (!AppValues.miniMode)
                versionPanel.Visible = !versionPanel.Visible;
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            Home_Click(this, null);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            Settings_Click(this, null);
        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            Plugins_Click(this, null);
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            Help_Click(this, null);
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            //if (VitNX3.Functions.Information.Internet.INTERNET_STATUS.CONNECTED == VitNX3.Functions.Information.Internet.IsHaveInternet())
            //{
            //    if (SysFile.Exists("PDK.exe")) { try { File.DeleteForever("PDK.exe"); } catch { } }
            //    label13.Text = $"Plugin Development Kit {label13.Text}";
            //    label13.Visible = true;
            //    VitNX3.Functions.Web.DataFromSites.DownloadFileWithSupportOfResume("https://files.witg.ru/capps/PDK.exe", "PDK.exe");
            //    label13.Visible = false;
            //    label13.Text = label13.Text.Replace("Plugin Development Kit ", "");
            //    Processes.Run("PDK.exe");
            //}
            //else
            //    try { Processes.Run("PDK.exe"); } catch { }
            if (Processes.OpenLink("https://github.com/Zalexanninev15/net-apps/blob/master/%D0%A1%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D0%B5%20%D1%81%D0%B2%D0%BE%D0%B5%D0%B3%D0%BE%20%D0%BF%D0%BB%D0%B0%D0%B3%D0%B8%D0%BD%D0%B0%20%D0%B4%D0%BB%D1%8F%20Shark%20Remote.md") == false)
                Processes.Open("https://github.com/Zalexanninev15/net-apps/blob/master/%D0%A1%D0%BE%D0%B7%D0%B4%D0%B0%D0%BD%D0%B8%D0%B5%20%D1%81%D0%B2%D0%BE%D0%B5%D0%B3%D0%BE%20%D0%BF%D0%BB%D0%B0%D0%B3%D0%B8%D0%BD%D0%B0%20%D0%B4%D0%BB%D1%8F%20Shark%20Remote.md");
        }

        private void getBotTokenNow_Click(object sender, EventArgs e)
        {
            IsChanged = true;
            AppValues.botToken = Clipboard.GetText();
        }

        private void vitnX2_Button1_Click(object sender, EventArgs e)
        {
            // Get current settings and comparison with new settings
            string user_old = AppSettings.Read("bot", "username");
            string user = username.Texts;
            if (user != "")
            {
                if (user.ToLower().StartsWith("https://t.me/"))
                    user = user.Replace("https://t.me/".ToLower(), "").Replace("http://t.me/".ToLower(), "").Replace("t.me/".ToLower(), "");
                else if (user.ToLower().EndsWith(".t.me"))
                    user = user.Replace(".t.me".ToLower(), "").Replace("https://".ToLower(), "").Replace("http://".ToLower(), "");
                else
                    user = user.StartsWith('@') ? user.Remove(0, 1) : user;
            }
            else
            {
                VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Не указан username пользователя!\nБудет использован старый username пользователя из настроек",
                        "Ошибка получения username",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                username.Texts = user_old;
                user = user_old;
            }
            bool use_rounded_window_frame_style = Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style"));
            string menu_color = AppSettings.Read("ui", "menu_color");
            bool use_window_transparency = Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency"));
            bool use_window_animation = Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation"));
            if (vitnX2_ToogleButton2.Checked == true)
            {
                use_window_transparency = false;
                use_window_animation = false;
                use_rounded_window_frame_style = false;
            }
            try
            {
                TomlTable toml = new TomlTable
                {
                    ["bot"] =
                    {
                        ["token"] = AppValues.botToken,
                        ["username"] = user
                    },
                    ["tprint"] =
                    {
                        ["font"] = TempFont,
                        ["size"] = TempSize
                    },
                    ["ui"] =
                    {
                        ["use_rounded_window_frame_style"] = use_rounded_window_frame_style,
                        ["use_window_mini_mode"] = AppValues.miniMode,
                        ["menu_color"] = menu_color,
                        ["use_window_transparency"] = use_window_transparency,
                        ["use_window_animation"] = use_window_animation,
                    }
                };
                using (StreamWriter writer = SysFile.CreateText($@"{Application.StartupPath}\data\settings\main.toml"))
                {
                    toml.WriteTo(writer);
                    writer.Flush();
                }
                var Dialog = VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Часть настроек будет применена после перезапуска приложения!\nЖелаете перезапустить?", "Настройки", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (Dialog == DialogResult.Yes) Application.Restart();
                IsChanged = false;
            }
            catch (Exception ex) { VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error); }
        }

        private void vitnX2_Button5_Click(object sender, EventArgs e)
        {
            Processes.Open($"\"{Application.StartupPath}data\\settings\"");
        }

        public string servicePathTool = $"{Application.StartupPath}service\\";

        private void vitnX2_Button4_Click(object sender, EventArgs e)
        {
            botPowerControl.Checked = !botPowerControl.Checked;
        }

        public bool IsNextStage = true;

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.KeyCode == Keys.M && e.Control)
            {
                if (AppValues.miniMode && IsNextStage)
                {
                    label21.Visible = false;
                    label22.Visible = false;
                    vitnX2_Panel4.Visible = false;
                    Size = new Size(757, 362);
                    Location = new Point(form_x, form_y);
                    StartPosition = FormStartPosition.CenterScreen;
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == true)
                    {
                        try
                        {
                            if (Convert.ToInt64(VitNX3.Functions.Information.Windows.GetWindowsCurrentBuildNumberFromRegistry()) >= 2200)
                                Region = VitNX3.Functions.WindowAndControls.Window.SetWindowsElevenStyleForWinForm(Handle, Width, Height);
                            else
                                Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
                        }
                        catch { Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15)); }
                    }
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
                        Opacity = 0.96;
                    else
                    {
                        if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                        {
                            Opacity = 0;
                            System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                            launch.Tick += new EventHandler((sender, e) =>
                            {
                                if ((Opacity += 0.05d) == 1)
                                    launch.Stop();
                            });
                            launch.Interval = 20;
                            launch.Start();
                        }
                    }
                    SharkIcon.Visible = false;
                    AppValues.miniMode = false;
                    IsNextStage = false;
                }
                if (!AppValues.miniMode && IsNextStage)
                {
                    label21.Visible = true;
                    label22.Visible = true;
                    vitnX2_Panel4.Visible = true;
                    Size = new Size(210, 229);
                    Location = new Point(form_x, form_y);
                    StartPosition = FormStartPosition.CenterScreen;
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == true)
                    {
                        try
                        {
                            if (Convert.ToInt64(VitNX3.Functions.Information.Windows.GetWindowsCurrentBuildNumberFromRegistry()) >= 2200)
                                Region = VitNX3.Functions.WindowAndControls.Window.SetWindowsElevenStyleForWinForm(Handle, Width, Height);
                            else
                                Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
                        }
                        catch { Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15)); }
                    }
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
                        Opacity = 0.96;
                    else
                    {
                        if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                        {
                            Opacity = 0;
                            System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                            launch.Tick += new EventHandler((sender, e) =>
                            {
                                if ((Opacity += 0.05d) == 1)
                                    launch.Stop();
                            });
                            launch.Interval = 20;
                            launch.Start();
                        }
                    }
                    SharkIcon.Visible = false;
                    AppValues.miniMode = true;
                    IsNextStage = false;
                }
                IsNextStage = true;
                e.Handled = true;
            }
            if (e.KeyCode == Keys.P && e.Control)
            {
                string selection = "Shark Remote";
                if (string.IsNullOrEmpty(selection))
                    return;
                var form = Helpers.Product.Chick.GetBitmapScreenshot(selection);
                if (form == null)
                    return;
                Clipboard.SetImage(form);
            }
            if (e.KeyCode == Keys.P && e.Alt && e.Control)
            {
                if (AppValues.ProductMode)
                {
                    vitnX_ComboBox1.DataSource = Helpers.Product.Chick.GetAllWindowHandleNames();
                    vitnX_ComboBox1.Visible = true;
                }
            }
        }

        private void vitnX2_PictureBox4_Click(object sender, EventArgs e)
        {
            if (AppValues.miniMode && IsNextStage)
            {
                label21.Visible = false;
                label22.Visible = false;
                vitnX2_Panel4.Visible = false;
                Size = new Size(757, 362);
                Location = new Point(form_x, form_y);
                StartPosition = FormStartPosition.CenterScreen;
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == true)
                {
                    try
                    {
                        if (Convert.ToInt64(VitNX3.Functions.Information.Windows.GetWindowsCurrentBuildNumberFromRegistry()) >= 2200)
                            Region = VitNX3.Functions.WindowAndControls.Window.SetWindowsElevenStyleForWinForm(Handle, Width, Height);
                        else
                            Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
                    }
                    catch { Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15)); }
                }
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
                    Opacity = 0.96;
                else
                {
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                    {
                        Opacity = 0;
                        System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                        launch.Tick += new EventHandler((sender, e) =>
                        {
                            if ((Opacity += 0.05d) == 1)
                                launch.Stop();
                        });
                        launch.Interval = 20;
                        launch.Start();
                    }
                }
                SharkIcon.Visible = false;
                AppValues.miniMode = false;
                IsNextStage = false;
            }
            if (!AppValues.miniMode && IsNextStage)
            {
                label21.Visible = true;
                label22.Visible = true;
                vitnX2_Panel4.Visible = true;
                Size = new Size(210, 229);
                Location = new Point(form_x, form_y);
                StartPosition = FormStartPosition.CenterScreen;
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_rounded_window_frame_style")) == true)
                {
                    try
                    {
                        if (Convert.ToInt64(VitNX3.Functions.Information.Windows.GetWindowsCurrentBuildNumberFromRegistry()) >= 2200)
                            Region = VitNX3.Functions.WindowAndControls.Window.SetWindowsElevenStyleForWinForm(Handle, Width, Height);
                        else
                            Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15));
                    }
                    catch { Region = Region.FromHrgn(Import.CreateRoundRectRgn(0, 0, Width, Height, 15, 15)); }
                }
                if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_transparency")))
                    Opacity = 0.96;
                else
                {
                    if (Convert.ToBoolean(AppSettings.Read("ui", "use_window_animation")))
                    {
                        Opacity = 0;
                        System.Windows.Forms.Timer launch = new System.Windows.Forms.Timer();
                        launch.Tick += new EventHandler((sender, e) =>
                        {
                            if ((Opacity += 0.05d) == 1)
                                launch.Stop();
                        });
                        launch.Interval = 20;
                        launch.Start();
                    }
                }
                SharkIcon.Visible = false;
                AppValues.miniMode = true;
                IsNextStage = false;
            }
            IsNextStage = true;
        }

        private void versionLabel_Click(object sender, EventArgs e)
        {
            progressVisual.Visible = true;
            Task.Run(async () => await AppUpdater.Upgrade());
        }

        private void vitnX_ComboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            vitnX_ComboBox1.Visible = false;
            string selection = vitnX_ComboBox1.SelectedValue.ToString();
            if (string.IsNullOrEmpty(selection))
                return;
            var form = Helpers.Product.Chick.GetBitmapScreenshot(selection);
            if (form == null)
                return;
            Clipboard.SetImage(form);
        }

        public static string TempFont = "Arial";
        public static int TempSize = 10;

        private void vitnX2_Button3_Click(object sender, EventArgs e)
        {
            FontDialog fd = new FontDialog();
            fd.ShowColor = false;
            fd.ShowEffects = false;
            fd.ShowHelp = false;
            FontConverter fc = new FontConverter();
            TempFont = Convert.ToString(AppSettings.Read("tpring", "font"));
            fd.Font = fc.ConvertFromString(TempFont) as Font;
            if (fd.ShowDialog() != DialogResult.Cancel)
            {
                TempFont = fc.ConvertToString(fd.Font).Split(';')[0];
                TempSize = (int)Math.Round(fd.Font.Size);
            }
        }

        static string versionLabelText = "Версия: 4.3.1 Beta (1510)\r\nРазработчик: Zalexanninev15";

        private void versionLabel_MouseEnter(object sender, EventArgs e)
        {
            versionLabel.ForeColor = Color.LimeGreen;
            versionLabel.Text = "Нажмите для проверки\nна наличие обновлений";
        }

        private void versionLabel_MouseLeave(object sender, EventArgs e)
        {
            versionLabel.ForeColor = Color.FromArgb(247, 247, 248);
            versionLabel.Text = versionLabelText;
        }

        private void vitnX2_ToogleButton2_CheckedChanged(object sender, EventArgs e)
        {
            if (!firstScCheck)
            {
                progressVisual.Visible = true;
                try { Directory.Delete(servicePathTool, true); } catch { }
                if (vitnX2_ToogleButton2.Checked)
                {
                    label2.Text = "Настраиваю...";
                    if (Network.InternetOk())
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        if (Directory.Exists(servicePathTool)) { try { Dir.DeleteForever(servicePathTool); } catch { } }
                        Directory.CreateDirectory("service");
                        label13.Text = $"Служба Windows {label13.Text}";
                        label13.Visible = true;
                        VitNX3.Functions.Web.DataFromSites.DownloadFileWithSupportOfResume("https://codeberg.org/Voocfof/open-api/raw/branch/main/Shark%20Remote/WinSW.exe", $"{servicePathTool}WinSW.exe.css");
                        SysFile.Copy($"{servicePathTool}WinSW.exe.css", $"{servicePathTool}WinSW.exe", true);
                        SysFile.WriteAllText($"{servicePathTool}WinSW.xml", Properties.Resources.WinSW);
                        string t = SysFile.ReadAllText($"{servicePathTool}WinSW.xml");
                        t = t.Replace("<workingdirectory>Path to Shark Remote.exe here", $"<workingdirectory>{Application.StartupPath}")
                            .Replace("<executable>Shark Remote", $"<executable>{Application.StartupPath}Shark Remote.exe");
                        SysFile.WriteAllText($"{servicePathTool}WinSW.xml", t);
                        SysFile.Delete($"{servicePathTool}WinSW.exe.css");
                        Processes.RunAW($"{servicePathTool}WinSW.exe", $"install", false);
                        using (RegistryKey SharkRemoteServer = Registry.LocalMachine.OpenSubKey(@"SYSTEM\ControlSet001\Control\Windows", true))
                            SharkRemoteServer.SetValue("NoInteractiveServices", 0);
                        Processes.RunAW($"{servicePathTool}WinSW.exe", $"start", false);
                        label13.Visible = false;
                        label13.Text = label13.Text.Replace("Служба Windows ", "");
                        Cursor.Current = Cursors.Default;
                        label2.Text = "Включено";
                        VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Служба установлена, но требуется выход из приложения и\nзапуск службы вручную из Служб Windows (только в первый раз).\nНазвание: Shark Remote (SRServer)", "Требуется ручная активация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else
                    {
                        if (Directory.Exists(servicePathTool))
                        {
                            Cursor.Current = Cursors.WaitCursor;
                            label13.Text = $"Служба Windows {label13.Text}";
                            label13.Visible = true;
                            VitNX3.Functions.Web.DataFromSites.DownloadFileWithSupportOfResume("https://codeberg.org/Voocfof/open-api/raw/branch/main/Shark%20Remote/WinSW.exe", $"{servicePathTool}WinSW.exe.css");
                            SysFile.Copy($"{servicePathTool}WinSW.exe.css", $"{servicePathTool}WinSW.exe", true);
                            SysFile.WriteAllText($"{servicePathTool}WinSW.xml", Properties.Resources.WinSW);
                            string t = SysFile.ReadAllText($"{servicePathTool}WinSW.xml");
                            t = t.Replace("<workingdirectory>Path to Shark Remote.exe here", $"<workingdirectory>{Application.StartupPath}");
                            SysFile.WriteAllText($"{servicePathTool}WinSW.xml", t);
                            SysFile.Delete($"{servicePathTool}WinSW.exe.css");
                            Processes.RunAW($"{servicePathTool}WinSW.exe", $"install", false);
                            using (RegistryKey SharkRemoteServer = Registry.LocalMachine.CreateSubKey(@"SYSTEM\ControlSet001\Control\Windows", true))
                                SharkRemoteServer.SetValue("NoInteractiveServices", 0);
                            Processes.RunAW($"{servicePathTool}WinSW.exe", $"start", false);
                            label13.Visible = false;
                            label13.Text = label13.Text.Replace("Служба Windows ", "");
                            Cursor.Current = Cursors.Default;
                            label2.Text = "Включено";
                            VitNX2.UI.ControlsV2.VitNX2_MessageBox.Show("Служба установлена, но требуется выход из приложения и\nзапуск службы вручную из Служб Windows (только в первый раз).\nНазвание: Shark Remote (SRServer)", "Требуется ручная активация", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        else
                            label2.Text = "Сеть отсутствует";
                    }
                }
                else
                {
                    try
                    {
                        Cursor.Current = Cursors.WaitCursor;
                        try { Processes.RunAW($"{servicePathTool}WinSW.exe", "stop", false); } catch { }
                        Processes.RunAW($"{servicePathTool}WinSW.exe", "uninstall", false);
                        Dir.DeleteForever(servicePathTool);
                        Cursor.Current = Cursors.Default;
                        label2.Text = "Отключено";
                    }
                    catch { label2.Text = "Ошибка"; }
                }
                progressVisual.Visible = false;
            }
            else
            {
                label2.Text = "Включено";
                firstScCheck = false;
            }
        }

        private void label2_Click(object sender, EventArgs e)
        {
            vitnX2_ToogleButton2.Checked = !vitnX2_ToogleButton2.Checked;
        }

        private void titleMinimize_MouseEnter(object sender, EventArgs e)
        {
            titleMinimize.ForeColor = Color.FromArgb(0, 144, 242);
            label22.ForeColor = Color.FromArgb(0, 144, 242);
        }

        private void titleMinimize_MouseLeave(object sender, EventArgs e)
        {
            titleMinimize.ForeColor = Color.FromArgb(183, 185, 191);
            label22.ForeColor = Color.FromArgb(183, 185, 191);
        }
    }
}