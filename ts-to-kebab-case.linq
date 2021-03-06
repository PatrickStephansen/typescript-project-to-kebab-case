<Query Kind="Program" />

void Main()
{
	var actions = RenameEntries(@"C:\Development\Discovery\discovery-group-life.2\src\common\src");
	actions.Dump();
}

// Define other methods and classes here
IEnumerable<Change> RenameEntries(string folderPath){
	var fileSystemEntries= Directory.EnumerateFileSystemEntries(folderPath);
	
	var camelCaseRegex = new Regex(@"(?<=[a-z]+)([A-Z])(?=[a-z]+)");
	
	var files = fileSystemEntries
		.Where(f=>File.Exists(f))
		.Select(f=> new FileInfo(f));
		
	foreach (var f in files.Where(f=>camelCaseRegex.IsMatch(f.Name))) 
	{
		var newName = camelCaseRegex.Replace(f.Name,"-$1").ToLower();
		yield return new Change(f.Name, newName);
		f.MoveTo(Path.Combine(f.Directory.FullName, newName));
	}
	
	foreach (var f in files)
	{
		var originalContents = File.ReadAllText(f.FullName);
		var contents = originalContents
			.ReplaceWithKebab(@"(?<=import .*'\..*)([A-Z])")
			.ReplaceWithKebab(@"(?<=styleUrls.*'\..*)([A-Z])")
			.ReplaceWithKebab(@"(?<=export.*'\..*)([A-Z])")
			.ReplaceWithKebab(@"(?<=templateUrl.*'\..*)([A-Z])");
			
		if (originalContents != contents)
		{
			File.WriteAllText(f.FullName, contents);
		}
	}
	
	var folders = fileSystemEntries
		.Where(f=>Directory.Exists(f))
		.Select(f=>new DirectoryInfo(f));
		
	foreach (var f in folders) 
	{
		if (camelCaseRegex.IsMatch(f.Name))
		{
			var newName = camelCaseRegex.Replace(f.Name,"-$1").ToLower();
			yield return new Change(f.Name, newName);
			f.MoveTo(Path.Combine(f.Parent.FullName, newName));
		}
		foreach (var childReplacement in RenameEntries(f.FullName)) 
		{
			yield return childReplacement;
		}
		
	}
}
static class StringReplacementExtentions
{
	public static string ReplaceWithKebab(this string content, string regex)
	{
		return Regex.Replace(content, regex, m=> "-" + m.Value.ToLower());
	}
}

class Change{
	public string Before{get;private set;}
	public string After{get;private set;}
	public Change(string before,string after)
	{
		Before = before;
		After = after;
	}
}
