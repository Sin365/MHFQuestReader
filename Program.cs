using System.Text;

namespace MHFQuestReader
{
	internal class Program
	{
		static string loc = Path.GetDirectoryName(AppContext.BaseDirectory) + "\\";

		const string InDir = "Input";
		const string OutDir = "Out";
		const string ListFile = "OutListFile.csv";
		const string Ver = "0.0.1";

		static Dictionary<uint, string> DictQuestID2Name = new Dictionary<uint, string>();

		static void Main(string[] args)
		{
			string title = $"MHFQuestReader Ver.{Ver} By 皓月云 axibug.com";
			Console.Title = title;
			Console.WriteLine(title);


			if (!Directory.Exists(loc + InDir))
			{
				Console.WriteLine("Input文件不存在");
				Console.ReadLine();
				return;
			}

			if (!Directory.Exists(loc + OutDir))
			{
				Console.WriteLine("Out文件不存在");
				Console.ReadLine();
				return;
			}

			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

			Console.WriteLine($"-----------原数据读取完毕-----------");

			string[] files = FileHelper.GetDirFile(loc + InDir);
			Console.WriteLine($"共{files.Length}个文件，是否处理? (y/n)");

			string yn = Console.ReadLine();
			if (yn.ToLower() != "y")
				return;

			int index = 0;
			int errcount = 0;
			for (int i = 0; i < files.Length; i++)
			{
				string FileName = files[i].Substring(files[i].LastIndexOf("\\"));

				if (!FileName.ToLower().Contains(".mib") && !FileName.ToLower().Contains(".bin"))
				{
					continue;
				}
				index++;

				Console.WriteLine($">>>>>>>>>>>>>>开始处理 第{index}个文件  {FileName}<<<<<<<<<<<<<<<<<<<");
				FileHelper.LoadFile(files[i], out byte[] data);
				if (ModifyQuest.ReadQuset(data, out string _QuestName, out List<string> Infos, out uint _QuestID))
				{
					DictQuestID2Name[_QuestID] = _QuestName;
					string newfileName = FileName + "_" + _QuestName + ".txt";
					string outstring = loc + OutDir + "\\" + newfileName;

					FileHelper.SaveFile(outstring, Infos.ToArray());
					Console.WriteLine($">>>>>>>>>>>>>>成功处理 第{index}个:{outstring}");
				}
				else
				{
					errcount++;
					Console.WriteLine($">>>>>>>>>>>>>>处理失败 第{index}个");
				}
			}

			Console.WriteLine($"已处理{files.Length}个文件，其中{errcount}个失败");


			string outliststr = string.Empty;
			foreach (var file in DictQuestID2Name)
			{
				outliststr += $"{file.Key},{file.Value}\n";
			}

			// 转换编码为UTF-8  
			byte[] utf8Bytes = Encoding.Convert(Encoding.GetEncoding("shift_jis"), Encoding.UTF8, Encoding.GetEncoding("shift_jis").GetBytes(outliststr));

			// 将字节数组转换回字符串  
			outliststr =  Encoding.UTF8.GetString(utf8Bytes);


			File.WriteAllText(loc + ListFile, outliststr);


			string[] tempkeys = LoadToSaveTemplate.DictTimeTypeCount.Keys.OrderBy(w => w).ToArray();

			foreach (var r in tempkeys)
			{
				Console.WriteLine(r + ":" + LoadToSaveTemplate.DictTimeTypeCount[r]);
			}

			Console.ReadLine();
		}
	}
}
