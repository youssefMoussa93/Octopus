# Octopus – Wiki

## Table of Contents

- [Purpose](#purpose)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Features](#features)
  - [Saved Repositories](#saved-repositories)
  - [Loading a Repository](#loading-a-repository)
  - [Viewing Worktrees](#viewing-worktrees)
  - [Creating a Worktree](#creating-a-worktree)
  - [Deleting a Worktree](#deleting-a-worktree)
  - [Pushing a Branch](#pushing-a-branch)
  - [Opening in an IDE](#opening-in-an-ide)
- [Data Storage](#data-storage)

---

## Purpose

Octopus is a web-based **Git Worktree Manager**. It gives developers a visual interface to create, list, and delete Git worktrees across multiple repositories — without touching the command line.

Git worktrees let you check out multiple branches of the same repo into separate directories at the same time. Octopus makes managing those worktrees fast and straightforward, especially when working across many feature branches simultaneously.

---

## Tech Stack

| Layer | Technology |
|---|---|
| Language | C# / .NET 10 |
| UI Framework | Blazor Server (ASP.NET Core) |
| Styling | Bootstrap 5 |
| Database | SQLite (via `Microsoft.Data.Sqlite`) |
| Git Integration | Git CLI (via `System.Diagnostics.Process`) |

---

## Getting Started

**Prerequisites:**
- .NET 10 SDK
- Git installed and available on `PATH`

**Run the app:**

```bash
cd Octopus
dotnet run
```

Then open `https://localhost:PORT` in your browser (the port is shown in the terminal output).

---

## Features

### Saved Repositories

On the home page, previously loaded repositories appear as quick-access cards at the top. Each card shows the repository name and its full path on hover.

- **Click a card** to select that repository and load its worktrees immediately.
- **Click the pencil icon** to rename the display name. Press `Enter` to confirm or `Escape` to cancel.
- **Click the X button** to remove it from the saved list (the repo itself is not affected).

Repositories are saved automatically the first time you load their worktrees.

---

### Loading a Repository

1. Click **Browse** inside the Repository card to open the folder picker.
2. Navigate the file system. Directories containing a `.git` folder are marked with a `git` badge.
3. Click a directory to select it, then click **Select**.
4. Click **Load** to list all worktrees in that repository.

You can also type a path directly into the input field instead of using the picker.

---

### Viewing Worktrees

After loading, all worktrees are displayed in a table with:

| Column | Description |
|---|---|
| Path | Absolute path to the worktree directory |
| Branch | Checked-out branch name. The main worktree is marked with a `main` badge. |
| Commit | Short (7-character) commit hash of the current HEAD |

---

### Creating a Worktree

1. Click **+ New Worktree** to expand the creation form.
2. Enter the **path** where the new worktree directory should be created.
3. Enter a **branch name**.
   - Leave **Create new branch** unchecked to check out an existing branch.
   - Check **Create new branch** to create a brand-new branch at the current HEAD.
4. Click **Create**. On success the form closes and the table refreshes.

If an error occurs (e.g. the branch doesn't exist or the path is already in use), an error message is shown inside the form.

---

### Deleting a Worktree

Click **Delete** on any non-main worktree row. A confirmation dialog appears.

- Check **Force delete** to remove the worktree even if it has uncommitted changes.
- Click **Confirm Delete** to proceed, or **Cancel** to abort.

The main worktree cannot be deleted.

---

### Pushing a Branch

Click **Push** on a worktree row to push its branch to `origin`. The button is disabled for worktrees in a detached HEAD state. A success or failure message appears inline below the row after the push completes.

---

### Opening in an IDE

Each worktree row has two IDE launch buttons:

- **Open in Rider** — Opens the worktree directory in JetBrains Rider.
- **Open in Visual Studio** — Opens the worktree directory in Visual Studio.

Both use the macOS `open -a` command under the hood.

---

## Data Storage

Saved repository metadata is stored in a local SQLite database:

- **Location:** `%APPDATA%/Octopus/repos.db` on Windows, or the equivalent application data directory on macOS/Linux.
- **Schema:** A single `Repos` table with columns: `Id`, `Path` (unique), `Name`, `AddedAt`.

No git data is stored by Octopus — all worktree information is read live from the Git CLI each time you load a repository.
