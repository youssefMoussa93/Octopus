using System.Diagnostics;

namespace Octopus.Services;

public class WorktreeInfo
{
    public string Path { get; set; } = "";
    public string Branch { get; set; } = "";
    public string Commit { get; set; } = "";
    public bool IsMainWorktree { get; set; }
}

public class GitResult
{
    public bool Success { get; set; }
    public string Output { get; set; } = "";
    public string Error { get; set; } = "";
}

public class GitService
{
    public async Task<(List<WorktreeInfo> Worktrees, string Error)> ListWorktreesAsync(string repoPath)
    {
        var result = await RunGitAsync(repoPath, "worktree list --porcelain");
        if (!result.Success)
            return ([], result.Error);

        var worktrees = new List<WorktreeInfo>();
        WorktreeInfo? current = null;

        foreach (var line in result.Output.Split('\n'))
        {
            if (line.StartsWith("worktree "))
            {
                if (current != null) worktrees.Add(current);
                current = new WorktreeInfo { Path = line[9..].Trim() };
            }
            else if (current != null && line.StartsWith("HEAD "))
                current.Commit = line[5..].Trim()[..7];
            else if (current != null && line.StartsWith("branch "))
                current.Branch = line[7..].Replace("refs/heads/", "").Trim();
            else if (current != null && line.Trim() == "detached")
                current.Branch = "(detached)";
        }

        if (current != null) worktrees.Add(current);
        if (worktrees.Count > 0) worktrees[0].IsMainWorktree = true;

        return (worktrees, "");
    }

    public async Task<GitResult> AddWorktreeAsync(string repoPath, string worktreePath, string branch, bool createNewBranch)
    {
        var args = createNewBranch
            ? $"worktree add -b \"{branch}\" \"{worktreePath}\""
            : $"worktree add \"{worktreePath}\" \"{branch}\"";
        return await RunGitAsync(repoPath, args);
    }

    public async Task<GitResult> RemoveWorktreeAsync(string repoPath, string worktreePath, bool force = false)
    {
        var args = force
            ? $"worktree remove --force \"{worktreePath}\""
            : $"worktree remove \"{worktreePath}\"";
        return await RunGitAsync(repoPath, args);
    }

    public async Task<GitResult> PushAsync(string repoPath, string branch, string remote = "origin")
    {
        return await RunGitAsync(repoPath, $"push {remote} \"{branch}\"");
    }

    public async Task<(List<string> Branches, string Error)> ListBranchesAsync(string repoPath)
    {
        var result = await RunGitAsync(repoPath, "branch --format=%(refname:short)");
        if (!result.Success) return ([], result.Error);
        var branches = result.Output
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(b => b.Trim())
            .Where(b => !string.IsNullOrEmpty(b))
            .ToList();
        return (branches, "");
    }

    public static void OpenInRider(string path)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "open",
            Arguments = $"-a Rider \"{path}\"",
            UseShellExecute = false,
            CreateNoWindow = true
        };
        Process.Start(psi);
    }

    private static async Task<GitResult> RunGitAsync(string workingDir, string arguments)
    {
        var psi = new ProcessStartInfo("git", arguments)
        {
            WorkingDirectory = workingDir,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)!;
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new GitResult
        {
            Success = process.ExitCode == 0,
            Output = output,
            Error = string.IsNullOrEmpty(error) ? "" : error.Trim()
        };
    }
}
