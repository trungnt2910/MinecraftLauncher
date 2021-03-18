using Microsoft.Win32;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Management.Automation;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;

namespace Launcher
{
	public partial class MainWindow : Window
	{
		private bool cancel2;

		private string archive;

		private int i;

		private bool cancel;

		private string[] blacklist;

		private bool owner;

		private string lastError;

		private Thread kj2;

		private Thread kj;

		private int coun;

		private int customed;

		private Thread ga;

		private bool stop;

		private bool waitneeded;

		private bool pressed;

		private Thread watcherThread;

		private Task runtimeBrokerManagerTask;

		private CancellationTokenSource runtimeBrokerManagerTaskCancelSource = new CancellationTokenSource();

		public bool RuntimeBrokerManagerEnabled
        {
			get => MinecraftRunning;
        }

		private int ActiveButtonMode
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => {
					this.play.IsEnabled = false;
					this.hack_disable.IsEnabled = false;
					this.install.IsEnabled = false;
					this.install2.IsEnabled = false;
					this.manager_enable.IsEnabled = RuntimeBrokerManagerEnabled;
					this.manager_disable.IsEnabled = RuntimeBrokerManagerEnabled;
					switch (value)
					{
						case 0:
							return;
						case 1:
						{
							this.play.IsEnabled = true;
							return;
						}
						case 2:
						{
							this.hack_disable.IsEnabled = true;
							return;
						}
						case 3:
						{
							this.install.IsEnabled = true;
							return;
						}
						case 4:
						{
							this.install2.IsEnabled = true;
							return;
						}
					}
					this.play.IsEnabled = true;
					this.hack_disable.IsEnabled = true;
					this.install.IsEnabled = true;
					this.install2.IsEnabled = true;
					this.manager_enable.IsEnabled = RuntimeBrokerManagerEnabled;
					this.manager_disable.IsEnabled = RuntimeBrokerManagerEnabled;
				});
			}
		}

		private bool install_mode
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => {
					if (value)
					{
						this.filer.Visibility = System.Windows.Visibility.Visible;
						this.par.Visibility = System.Windows.Visibility.Visible;
						this.progress.Visibility = System.Windows.Visibility.Visible;
						return;
					}
					this.filer.Visibility = System.Windows.Visibility.Hidden;
					this.par.Visibility = System.Windows.Visibility.Visible;
					this.progress.Visibility = System.Windows.Visibility.Hidden;
				});
			}
		}

		private bool install1_ended
		{
			set
			{
				if (value)
				{
					System.Windows.Application.Current.Dispatcher.Invoke(() => {
						this.kj = null;
						this.ActiveButtonMode = 10;
						this.install.Content = "Install/Update Minecraft";
						this.cancel = false;
					});
				}
			}
		}

		private bool install2_ended
		{
			set
			{
				if (value)
				{
					System.Windows.Application.Current.Dispatcher.Invoke(() => {
						this.kj2 = null;
						this.ActiveButtonMode = 89;
						this.install2.Content = "Install appx at custom location";
						this.cancel2 = false;
					});
				}
			}
		}

		private string MainLogger
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => {
					this.home_log.AppendText(string.Concat("\n", value));
					this.home_log.ScrollToEnd();
					this.home_log.UpdateLayout();
				});
			}
		}

		private int Maximum
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => this.progress.Maximum = (double)value);
			}
		}

        private bool MinecraftRunning
        {
            get
            {
                try
                {
					var processes = Tasklist.GetProcessesByImageName("Minecraft.Windows.exe");
					return processes.Count > 0;
				}
				catch
                {
					return false;
                }
            }
        }

        private int progress_
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => this.progress.Value = (double)value);
			}
		}

		private int runcount
		{
			get
			{
				return this.coun;
			}
			set
			{
				switch (value)
				{
					case 0:
					case 1:
					{
						this.coun = 660;
						return;
					}
					case 2:
					{
						this.coun = 660;
						return;
					}
					case 3:
					{
						this.coun = 660;
						return;
					}
					case 4:
					{
						this.coun = 660;
						return;
					}
				}
				this.coun = 660;
			}
		}

		private string status
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => this.filer.Text = value);
			}
		}

		private bool status_st
		{
			set
			{
				System.Windows.Application.Current.Dispatcher.Invoke(() => {
					if (value)
					{
						this.filer.Visibility = System.Windows.Visibility.Visible;
						this.par.Visibility = System.Windows.Visibility.Visible;
						return;
					}
					this.filer.Visibility = System.Windows.Visibility.Hidden;
					this.par.Visibility = System.Windows.Visibility.Hidden;
				});
			}
		}

		/// <summary>
		/// Function that check's if current user is in Aministrator role
		/// </summary>
		/// <returns></returns>
		public static bool IsRunningAsAdministrator()
		{
			// Get current Windows user
			WindowsIdentity windowsIdentity = WindowsIdentity.GetCurrent();

			// Get current Windows user principal
			WindowsPrincipal windowsPrincipal = new WindowsPrincipal(windowsIdentity);

			// Return TRUE if user is in role "Administrator"
			return windowsPrincipal.IsInRole(WindowsBuiltInRole.Administrator);
		}

		public MainWindow()
		{
			if (!IsRunningAsAdministrator())
			{
				// Setting up start info of the new process of the same application
				ProcessStartInfo processStartInfo = new ProcessStartInfo(Assembly.GetEntryAssembly().CodeBase);

				// Using operating shell and setting the ProcessStartInfo.Verb to “runas” will let it run as admin
				processStartInfo.UseShellExecute = true;
				processStartInfo.Verb = "runas";

				// Start the application as new process
				Process.Start(processStartInfo);

				// Shut down the current (old) process
				System.Windows.Application.Current.Shutdown();
			}
			else
            {
				this.InitializeComponent();
				RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\ClipSVC\\Parameters", true);
				RegistrySecurity registrySecurity = new RegistrySecurity();
				RegistrySecurity accessControl = registryKey.GetAccessControl();
				string str = string.Concat(Environment.UserDomainName, "\\", Environment.UserName);
				accessControl.AddAccessRule(new RegistryAccessRule(str, RegistryRights.FullControl, AccessControlType.Allow));
				accessControl.AddAccessRule(new RegistryAccessRule("Administrators", RegistryRights.FullControl, AccessControlType.Allow));
			}
		}

		private async void a_play_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(() => { });
			//MainWindow.<a_play_Click>d__77 variable = new MainWindow.<a_play_Click>d__77();
			//variable.<>t__builder = AsyncVoidMethodBuilder.Create();
			//variable.<>1__state = -1;
			//variable.<>t__builder.Start<MainWindow.<a_play_Click>d__77>(ref variable);
		}

		private async void Button_Click(object sender, RoutedEventArgs e)
		{
			await Task.Run(() => { });
			//MainWindow.<Button_Click>d__51 variable = new MainWindow.<Button_Click>d__51();
			//variable.<>t__builder = AsyncVoidMethodBuilder.Create();
			//variable.<>1__state = -1;
			//variable.<>t__builder.Start<MainWindow.<Button_Click>d__51>(ref variable);
		}

		private void cmdprompt_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				this.pressed = false;
			}
		}

		private void error_copy_Click(object sender, RoutedEventArgs e)
		{
			if (string.IsNullOrEmpty(this.lastError))
			{
				this.MainLogger = "No error to copy";
				return;
			}
			System.Windows.Clipboard.SetText(this.lastError);
			this.MainLogger = "Copied";
		}

		private void Error_DataAdded(object sender, DataAddedEventArgs e)
		{
			this.MainLogger = ((PSDataCollection<ErrorRecord>)sender)[e.Index].FullyQualifiedErrorId;
		}

		private async void hack_disable_Click(object sender, RoutedEventArgs e)
		{
			this.ActiveButtonMode = 2;
			this.hack_disable.IsEnabled = false;
			await this.unregedit();
			await this.service1();
			this.home_log.AppendText("\n The hack has been disabled. Restart might be required");
			this.ActiveButtonMode = 200;
		}

		public async Task HandleManifest(string path)
		{
			await Task.Run(() => {
				int hResult;
				try
				{
					string[] strArrays = File.ReadAllLines(path);
					int num = 0;
					string str = "";
					string[] strArrays1 = strArrays;
					hResult = 0;
					while (true)
					{
						if (hResult < (int)strArrays1.Length)
						{
							string str1 = strArrays1[hResult];
							if (!str1.Contains("PhonePublisherId"))
							{
								num++;
								hResult++;
							}
							else
							{
								str = str1;
								break;
							}
						}
						else
						{
							this.MainLogger = "The appx package was not recognised correctly. Please report at discord";
							break;
						}
					}
					string str2 = str.Substring(str.IndexOf("PhonePublisherId=") + 17);
					str2 = str2.Replace(" ", "");
					str = str.Replace(str2.Remove(38), "\"00000000-0000-0000-0000-000000000000\"");
					strArrays[num] = str;
					File.WriteAllLines(path, strArrays);
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					MainWindow u003cu003e4_this = this;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					u003cu003e4_this.MainLogger = string.Concat(message);
					MainWindow mainWindow = this;
					string[] source = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					source[2] = hResult.ToString();
					source[3] = "\nThe source is ";
					source[4] = exception.Source;
					mainWindow.lastError = string.Concat(source);
				}
			});
		}

		private Version GetInstalledMinecraftVersion()
        {
            try
            {
				PowerShell shell = PowerShell.Create();
				var returnValue = shell.AddCommand("Get-AppxPackage").AddParameter("Name", "Microsoft.MinecraftUWP").Invoke();
				PSObject obj = returnValue.First();
				return Version.Parse((string)obj.Properties["Version"].Value);
			}
			catch
			{

            }
			return null;
        }

		private async void install_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
			if (button.Content == null)
			{
				this.cancel = true;
				this.MainLogger = "Remaining install list will be cancelled";
				button.IsEnabled = false;
				return;
			}

            try
            {
				ActiveButtonMode = 3;
				button.Dispatcher.Invoke(() => button.Content = "Cancel");
				
				MicrosoftStoreApp app = new MicrosoftStoreApp("9nblggh2jhxj");

				MainLogger = "Fetching packages...";
				await app.LoadAsync();
				MainLogger = $"Fetched {app.Locations.Count} packages.";

				var location = app.Find("Microsoft.MinecraftUWP", Environment.Is64BitOperatingSystem ? Architecture.x64 : Architecture.x86);
				MainLogger = $"Latest version is {location.Version}";

				var currentVersion = GetInstalledMinecraftVersion();

				if ((currentVersion != null) && (currentVersion >= location.Version))
                {
					MainLogger = "Latest version installed";
					install1_ended = true;
					return;
                }

				install_mode = true;

				this.status = "Downloading app package...";
				string fileLocation = await location.DownloadAsync((args) => {
					progress_ = args.ProgressPercentage;
					status = $"Downloaded: {args.BytesReceived}/{args.TotalBytesToReceive}";
				});

				MainLogger = $"Appx saved to {fileLocation}";
				
				
				kj = new Thread(new ParameterizedThreadStart(this.moded));
				kj.Start(new string[] { fileLocation });
			}
			catch (Exception ex)
            {
				MainLogger = ex.ToString();
				install_mode = false;
            }
			finally
            {
				ActiveButtonMode = 7;
            }
			return;
		}

		private void install2_Click(object sender, RoutedEventArgs e)
		{
			System.Windows.Controls.Button button = (System.Windows.Controls.Button)sender;
			if (button.Content == null)
			{
				this.cancel2 = true;
				button.IsEnabled = false;
				return;
			}
			Microsoft.Win32.OpenFileDialog openFileDialog = new Microsoft.Win32.OpenFileDialog()
			{
				Filter = "UWP files|*.appx",
				Title = "Select UWP file to install",
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false
			};
			bool? nullable = openFileDialog.ShowDialog();
			if (nullable.GetValueOrDefault() & nullable.HasValue)
			{
				FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog()
				{
					Description = "Select the folder where you want to install the app",
					ShowNewFolderButton = true
				};
				if (folderBrowserDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
				{
					this.ActiveButtonMode = 4;
					button.Content = "Cancel";
					this.kj2 = new Thread(new ParameterizedThreadStart(this.moder));
					this.kj2.Start(new string[] { openFileDialog.FileName, folderBrowserDialog.SelectedPath });
				}
			}
		}

		public async Task<bool> IsInstalled(string package)
		{
			bool flag;
			string str;
			try
			{
				PowerShell powerShell = PowerShell.Create();
				powerShell.AddCommand("Get-AppxPackage");
				powerShell.AddParameter("Name", package);
				this.MainLogger = "Wait a few moments";
				Collection<PSObject> pSObjects = null;
				await Task.Run(() => pSObjects = powerShell.Invoke());
				if (pSObjects.Count != 0)
				{
					MainWindow mainWindow = this;
					object value = pSObjects[0].Members["Version"].Value;
					if (value != null)
					{
						str = value.ToString();
					}
					else
					{
						str = null;
					}
					mainWindow.MainLogger = string.Concat("Minecraft with version ", str, " will be launched");
					flag = true;
					return flag;
				}
				else
				{
					flag = false;
					return flag;
				}
			}
			catch (Exception exception1)
			{
				Exception exception = exception1;
				string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
				int hResult = exception.HResult;
				message[2] = hResult.ToString();
				message[3] = "\nThe source is ";
				message[4] = exception.Source;
				this.MainLogger = string.Concat(message);
				string[] source = new string[] { exception.Message, "\nThe error code is ", null, null, null };
				hResult = exception.HResult;
				source[2] = hResult.ToString();
				source[3] = "\nThe source is ";
				source[4] = exception.Source;
				this.lastError = string.Concat(source);
			}
			flag = false;
			return flag;
		}

		// FixMe: Process Explorer has a way to identify which package a RuntimeBroker is attached to.
		// We should implement this to guarantee the client's system instability.
		public async Task KillRuntimeBroker(CancellationToken? token = null)
		{
			await Task.Run(() => {
				try
				{
					DateTime? startTime = null;

					int failCount = 0;

					while (failCount < 10)
					{
						var p = Tasklist.GetProcessesByImageName("Minecraft.Windows.exe");
						if (p.Count != 1)
						{
							MainLogger = "Waiting for Minecraft to start...";
							Thread.Sleep(1000);
							++failCount;
							continue;
						}
						else
						{
							startTime = p[0].StartTime;
							break;
						}
					}

					if (startTime == null)
                    {
						MainLogger = "Failed to start Minecraft.";
						this.owner = false;
						this.ActiveButtonMode = 6;
						MainLogger = "Minecraft has stopped.";
						manager_disable_Click(null, null);
						return;
					}

					for (int i = 0; i < 128; ++i)
					{
						if (token?.IsCancellationRequested == true)
						{
							MainLogger = "RuntimeBroker killer stopped.";
							return;
						}

						var processes = Tasklist.GetUwpAppsByImageName("RuntimeBroker.exe");
						foreach (var kvp in processes)
                        {
							if (kvp.Item2.Contains("Microsoft.MinecraftUWP"))
                            {
                                try
                                {
									if (!kvp.Item1.HasExited)
										kvp.Item1.Kill();
									MainLogger = $"Killed a broker with PID {kvp.Item1.Id}, with process name: {kvp.Item1.ProcessName}, running as part of package {kvp.Item2}";
								}
								catch 
                                {
									MainLogger = $"Failed to kill the broker with PID {kvp.Item1.Id}, from package {kvp.Item2}. The game might not load correctly.";
                                }
                            }
						}

						Thread.Sleep(1000);
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					i = exception.HResult;
					message[2] = i.ToString();
					message[3] = "\n";
					message[4] = exception.ToString();
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					i = exception.HResult;
					str[2] = i.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
				finally
                {
					runtimeBrokerManagerTask = null;
					runtimeBrokerManagerTaskCancelSource.Dispose();
					runtimeBrokerManagerTaskCancelSource = null;
                }
			});
		}

		public async Task killer1()
		{
			await Task.Run(() => {
				int i;
				try
				{
					Process[] processesByName = Process.GetProcessesByName("RuntimeBroker");
					for (i = 0; i < (int)processesByName.Length; i++)
					{
						processesByName[i].Kill();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					i = exception.HResult;
					message[2] = i.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					i = exception.HResult;
					str[2] = i.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
			});
		}

		private void link1_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://www.youtube.com/channel/UCgaludRTCVuByNy6Gx7AknQ");
		}

		private void link2_Click(object sender, RoutedEventArgs e)
		{
			Process.Start("https://discord.gg/92QEYVa");
		}

		public async Task mode1(string[] files)
		{
			await this.unregedit();
			await this.service1();
			this.Maximum = 100;
			this.install_mode = true;
			string[] strArrays = files;
			int num = 0;
			while (true)
			{
				if (num < (int)strArrays.Length)
				{
					string str = strArrays[num];
					this.progress_ = 0;
					if (this.cancel)
					{
						break;
					}
					string[] strArrays1 = new string[] { "\\", "/" };
					string[] strArrays2 = str.Split(strArrays1, StringSplitOptions.RemoveEmptyEntries);
					if (File.Exists(str))
					{
						this.status = string.Concat("Now installing ", strArrays2[(int)strArrays2.Length - 1]);
						this.MainLogger = string.Concat("Now installing ", strArrays2[(int)strArrays2.Length - 1]);
						PowerShell powerShell = PowerShell.Create();
						powerShell.AddCommand("Add-AppxPackage");
						powerShell.AddParameter("Path", str);
						powerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
						powerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.Progress_DataAdded);
						await Task.Run(() => powerShell.Invoke());
						if (!powerShell.HadErrors)
						{
							this.undo();
							this.MainLogger = string.Concat("Installed ", strArrays2[(int)strArrays2.Length - 1]);
							this.status = "";
						}
						strArrays2 = null;
						num++;
					}
					else
					{
						return;
					}
				}
				else
				{
					strArrays = null;
					break;
				}
			}
			this.install_mode = false;
			this.install1_ended = true;
		}

		private async Task mode2(string install)
		{
			await this.unregedit();
			await this.service1();
			this.install_mode = true;
			this.Maximum = 100;
			this.progress_ = 0;
			if (File.Exists(install))
			{
				this.status = "Registering";
				this.MainLogger = "Started Registration process";
				PowerShell powerShell = PowerShell.Create();
				powerShell.AddCommand("Add-AppxPackage");
				powerShell.AddParameter("Path", install);
				powerShell.AddParameter("Register");
				powerShell.Streams.Error.DataAdded += new EventHandler<DataAddedEventArgs>(this.Error_DataAdded);
				powerShell.Streams.Progress.DataAdded += new EventHandler<DataAddedEventArgs>(this.Progress_DataAdded);
				await Task.Run(() => powerShell.Invoke());
				if (!powerShell.HadErrors)
				{
					this.undo();
					this.MainLogger = "Package Registered";
					this.status = "";
				}
				this.install_mode = false;
				this.install2_ended = true;
			}
		}

		private async void moded(object files)
		{
			await this.mode1((string[])files);
		}

		private async void moder(object parameter)
		{
			string[] strArrays = (string[])parameter;
			await this.unPackage(strArrays[0], strArrays[1]);
			if (!this.cancel2)
			{
				if (strArrays[1].EndsWith("/") || strArrays[1].EndsWith("\\"))
				{
					await this.HandleManifest(string.Concat(strArrays[1], "AppxManifest.xml"));
				}
				else
				{
					await this.HandleManifest(string.Concat(strArrays[1], "\\AppxManifest.xml"));
				}
				if (!this.cancel2)
				{
					if (strArrays[1].EndsWith("/") || strArrays[1].EndsWith("\\"))
					{
						File.Delete(string.Concat(strArrays[1], "AppxSignature.p7x"));
						await this.mode2(string.Concat(strArrays[1], "AppxManifest.xml"));
					}
					else
					{
						File.Delete(string.Concat(strArrays[1], "\\AppxSignature.p7x"));
						await this.mode2(string.Concat(strArrays[1], "\\AppxManifest.xml"));
					}
				}
			}
			this.install2_ended = true;
		}

		private async Task parser(string cmd)
		{
			string str = cmd;
			string[] strArrays = new string[] { " " };
			string[] strArrays1 = str.Split(strArrays, StringSplitOptions.RemoveEmptyEntries);
			string str1 = strArrays1[0];
			if (str1 != null)
			{
				if (str1 == ".timer")
				{
					await this.timer(strArrays1);
					return;
				}
				else if (str1 == ".repeater")
				{
					await this.repeatz(strArrays1);
					return;
				}
				else
				{
					if (str1 != ".help")
					{
						this.MainLogger = "--------------------------------------------------------------------------------";
						this.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
						return;
					}
					this.MainLogger = "--------------------------------------------------------------------------------";
					this.MainLogger = ".timer = value in seconds\t adjust timer according to your own choice";
					this.MainLogger = ".repeater = on or off    \t turns on or off the repeater";
					return;
				}
			}
			this.MainLogger = "--------------------------------------------------------------------------------";
			this.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
			return;
		}

		private async void play_Click(object sender, RoutedEventArgs e)
		{
			this.stop = true;
			this.runcount = 0;
			this.ActiveButtonMode = 1;
			this.play.IsEnabled = false;
			if (!await this.IsInstalled("Microsoft.MinecraftUWP"))
			{
				this.MainLogger = "Minecraft is not installed. Please install it using the Launcher, the Microsoft Store, or download and install a package online.";
				this.ActiveButtonMode = 10;
			}
			else if (!this.MinecraftRunning || this.owner)
			{
				this.ActiveButtonMode = 0;
				this.manager_enable.IsEnabled = true;
				this.manager_disable.IsEnabled = true;
				await this.regedit();
				await this.service();
				await this.LaunchMinecraft();
				this.owner = true;
				//while (this.waitneeded)
				//{
				//	Task.Delay(1).Wait();
				//}
				//this.waitneeded = false;
				//this.stop = false;
				//this.ga = new Thread(new ThreadStart(this.repeater));
				//this.main_logger = "Repeater started";
				//this.ga.Start();
				manager_enable_Click(null, null);
			}
			else
			{
				this.MainLogger = "Close Minecraft first";
				this.ActiveButtonMode = 6;
			}
		}

		private void Progress_DataAdded(object sender, DataAddedEventArgs e)
		{
			this.progress_ = ((PSDataCollection<ProgressRecord>)sender)[e.Index].PercentComplete;
		}

		private void progress_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
		}

		public async Task regedit()
		{
			await Task.Run(() => {
				int hResult;
				try
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\ClipSVC\\Parameters", true);
					hResult = (new Random()).Next(1, 1000);
					registryKey.SetValue("ServiceDll", string.Concat("%SystemRoot%\\System32\\ClipSVC.dll", hResult.ToString()), RegistryValueKind.ExpandString);
					registryKey.Close();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					str[2] = hResult.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
			});
		}

		private async void repeater()
		{
			int num = 0;
			while (!this.stop)
			{
				if (this.customed != 0)
				{
					await this.wait1(this.customed);
				}
				else
				{
					num++;
					this.runcount = num;
					await this.wait1(this.runcount);
				}
				await this.killer1();
			}
		}

		private async Task repeatz(string[] args)
		{
			MainWindow thread = this;
			thread.MainLogger = "--------------------------------------------------------------------------------";
			if ((int)args.Length != 3 || !args.Contains<string>("="))
			{
				thread.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
			}
			else if (args[1] != "=" || args[2] != "on" && args[2] != "off")
			{
				thread.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
			}
			else if (args[2] != "on")
			{
				thread.stop = true;
				thread.MainLogger = "Repeater stopped";
				thread.ga = null;
			}
			else if (thread.stop || thread.ga == null)
			{
				thread.ga = new Thread(new ThreadStart(thread.repeater));
				thread.stop = false;
				thread.MainLogger = "Repeater started";
				thread.ga.Start();
			}
			else
			{
				thread.MainLogger = "Already running";
			}
		}

		public async Task service()
		{
			await Task.Run(() => {
				ServiceController serviceController = new ServiceController("ClipSVC");
				try
				{
					if (serviceController.Status != ServiceControllerStatus.Stopped)
					{
						serviceController.Stop();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					int hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					str[2] = hResult.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
			});
		}

		public async Task service1()
		{
			await Task.Run(() => {
				ServiceController serviceController = new ServiceController("ClipSVC");
				try
				{
					if (serviceController.Status != ServiceControllerStatus.Running)
					{
						serviceController.Start();
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					int hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					str[2] = hResult.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
			});
		}

		private async void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.Return)
			{
				if (!string.IsNullOrWhiteSpace(this.cmdprompt.Text) && !this.pressed)
				{
					this.pressed = true;
					await this.parser(this.cmdprompt.Text);
				}
				this.cmdprompt.Text = "";
			}
		}

		private async Task timer(string[] args)
		{
			MainWindow mainWindow = this;
			mainWindow.MainLogger = "--------------------------------------------------------------------------------";
			uint num = 0;
			if ((int)args.Length != 3 || !args.Contains<string>("="))
			{
				mainWindow.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
			}
			else if (!uint.TryParse(args[2], out num))
			{
				mainWindow.MainLogger = "The command entered is invalid or format is not correct. Type \".help\" to get list of available commands";
			}
			else if (num >= 60 || num <= 0)
			{
				mainWindow.MainLogger = "timer updated";
				mainWindow.customed = (int)num;
			}
			else
			{
				mainWindow.MainLogger = "Set the value to 0 to get default settings, or it should be greater than 60";
			}
		}

		private void undo()
		{
			System.Windows.Application.Current.Dispatcher.Invoke(() => this.home_log.Undo());
		}

		private async Task unPackage(string file, string path)
		{
			this.install_mode = true;
			await Task.Run(() => {
				string[] strArrays = file.Split(new string[] { "\\", "/" }, StringSplitOptions.RemoveEmptyEntries);
				this.archive = strArrays[(int)strArrays.Length - 1];
				using (ZipArchive zipArchive = ZipFile.OpenRead(file))
				{
					this.status = string.Concat("Extracting ", this.archive);
					this.MainLogger = string.Concat("Extracting ", this.archive);
					this.Maximum = zipArchive.Entries.Count;
					this.progress_ = 0;
					this.i = 0;
					foreach (ZipArchiveEntry entry in zipArchive.Entries)
					{
						if (!this.cancel2)
						{
							try
							{
								entry.ExtractToFile(Path.Combine(path, entry.FullName), true);
							}
							catch (IOException oException1)
							{
								IOException oException = oException1;
								if (oException.Message.Contains("already exists"))
								{
									File.Delete(oException.Message.Replace("The file '", "").Replace("' already exists.", ""));
								}
								Directory.CreateDirectory(Path.GetDirectoryName(Path.Combine(path, entry.FullName)));
								entry.ExtractToFile(Path.Combine(path, entry.FullName), true);
							}
							this.i++;
							this.progress_ = this.i;
						}
						else
						{
							this.undo();
							this.install_mode = false;
							if (!this.cancel2)
							{
								this.MainLogger = string.Concat("Extracted ", this.archive);
								return;
							}
							this.MainLogger = string.Concat("Cancelled Extraction of ", this.archive);
							return;
						}
					}
				}
				this.undo();
				this.install_mode = false;
				if (!this.cancel2)
				{
					this.MainLogger = string.Concat("Extracted ", this.archive);
					return;
				}
				this.MainLogger = string.Concat("Cancelled Extraction of ", this.archive);
			});
		}

		public async Task unregedit()
		{
			await Task.Run(() => {
				try
				{
					RegistryKey registryKey = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\ClipSVC\\Parameters", true);
					registryKey.SetValue("ServiceDll", "%SystemRoot%\\System32\\ClipSVC.dll", RegistryValueKind.ExpandString);
					registryKey.Close();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					int hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					this.MainLogger = string.Concat(message);
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					str[2] = hResult.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					this.lastError = string.Concat(str);
				}
			});
		}

		public async Task LaunchMinecraft()
		{
			await Task.Run(() => {
				int id;
				try
				{
					this.status_st = true;
					Process[] processesByName = Process.GetProcessesByName("RuntimeBroker");
					this.blacklist = new string[(int)processesByName.Length];
					for (int i = 0; i != (int)processesByName.Length; i++)
					{
						string[] str = this.blacklist;
						id = processesByName[i].Id;
						str[i] = id.ToString();
					}
					Process.Start("minecraft://");
                    try
					{
						while (!MinecraftRunning)
						{
							MainLogger = "Waiting for Minecraft to start...";
							Thread.Sleep(1000);
						}
					}
					catch (Exception e)
                    {
						this.MainLogger = e.ToString();
                    }
					this.status_st = false;
					this.MainLogger = "Minecraft launched. If you are stuck at loading with Minecraft logo, try to click on Unfreeze Minecraft";
					watcherThread = new Thread(new ThreadStart(() =>
					{
						try
                        {
							while (MinecraftRunning)
							{
								Thread.Sleep(1000);
							}
							this.owner = false;
							this.ActiveButtonMode = 6;
							MainLogger = "Minecraft has stopped.";
							manager_disable_Click(null, null);
						}
						catch (ThreadInterruptedException e)
                        {
							return;
                        }
					}));
					watcherThread.Start();
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					id = exception.HResult;
					message[2] = id.ToString();
					message[3] = "\n ";
					message[4] = exception.ToString();
					this.MainLogger = string.Concat(message);
					string[] source = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					id = exception.HResult;
					source[2] = id.ToString();
					source[3] = "\nThe source is ";
					source[4] = exception.Source;
					this.lastError = string.Concat(source);
				}
			});
		}

		public async Task wait1(int wait)
		{
			await Task.Run(() => {
				int hResult;
				try
				{
					this.status_st = true;
					int num = wait;
					while (true)
					{
						if (num != 0)
						{
							if (this.stop)
							{
								break;
							}
							Task.Delay(1000).Wait();
							if (wait <= 20 || num <= 59)
							{
								this.status = string.Concat("repeating task in ", num.ToString(), " seconds");
							}
							else
							{
								MainWindow u003cu003e4_this = this;
								hResult = num / 60;
								u003cu003e4_this.status = string.Concat("repeating task in ", hResult.ToString(), " minutes");
							}
							num--;
						}
						else
						{
							this.status_st = false;
							this.MainLogger = "task repeated";
							break;
						}
					}
				}
				catch (Exception exception1)
				{
					Exception exception = exception1;
					MainWindow mainWindow = this;
					string[] message = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					message[2] = hResult.ToString();
					message[3] = "\nThe source is ";
					message[4] = exception.Source;
					mainWindow.MainLogger = string.Concat(message);
					MainWindow u003cu003e4_this1 = this;
					string[] str = new string[] { exception.Message, "\nThe error code is ", null, null, null };
					hResult = exception.HResult;
					str[2] = hResult.ToString();
					str[3] = "\nThe source is ";
					str[4] = exception.Source;
					u003cu003e4_this1.lastError = string.Concat(str);
				}
				this.waitneeded = false;
			});
		}

		private void Window_DragLeave(object sender, System.Windows.DragEventArgs e)
		{
			this.drop.Visibility = System.Windows.Visibility.Hidden;
			this.home.Visibility = System.Windows.Visibility.Visible;
			e.Handled = true;
		}

		private void Window_DragEnter(object sender, System.Windows.DragEventArgs e)
        {

        }

		private void Window_Drop(object sender, System.Windows.DragEventArgs e)
		{
			e.Effects = System.Windows.DragDropEffects.None;
			if (e.Data.GetDataPresent(System.Windows.DataFormats.FileDrop))
			{
				string[] data = (string[])e.Data.GetData(System.Windows.DataFormats.FileDrop);
			}
		}

        private void manager_enable_Click(object sender, RoutedEventArgs e)
        {
			if (runtimeBrokerManagerTask != null)
            {
				return;
            }
			runtimeBrokerManagerTask = KillRuntimeBroker(runtimeBrokerManagerTaskCancelSource.Token);
        }

		private async void manager_disable_Click(object sender, RoutedEventArgs e)
		{
			manager_enable.Dispatcher.Invoke(() => manager_enable.IsEnabled = false);
			runtimeBrokerManagerTaskCancelSource.Cancel();
			if (runtimeBrokerManagerTask != null) await runtimeBrokerManagerTask;
			runtimeBrokerManagerTaskCancelSource?.Dispose();
			runtimeBrokerManagerTaskCancelSource = new CancellationTokenSource();
			manager_enable.Dispatcher.Invoke(() => manager_enable.IsEnabled = RuntimeBrokerManagerEnabled);
		}

        private void Homepage_Click(object sender, RoutedEventArgs e)
        {
			Process.Start("https://github.com/trungnt2910/MinecraftLauncher");
		}

        private void Window_Closing(object sender, CancelEventArgs e)
        {
			manager_disable_Click(null, null);
			watcherThread?.Interrupt();
        }
    }
}
