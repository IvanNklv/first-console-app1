using EntityFrameworkTasks;
using EntityFrameworkTasks.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;

await using var db = new AppDbContext();

// Helper для приємного друку
void H(string title)
{
    Console.WriteLine();
    Console.WriteLine(new string('=', 80));
    Console.WriteLine(title);
    Console.WriteLine(new string('=', 80));
}

// 1) Print info about each project (Name, Description, Tasks Count, Members Count)
H("1) Projects info");
var q1 = await db.Projects
    .Select(p => new
    {
        p.Name,
        p.Description,
        TasksCount = p.Tasks.Count(),
        MembersCount = p.Members.Count()
    })
    .ToListAsync();
q1.ForEach(x => Console.WriteLine($"{x.Name} | tasks: {x.TasksCount} | members: {x.MembersCount} | {x.Description}"));

// 2) Find all tasks with more than 2 comments
H("2) Tasks with > 2 comments");
var q2 = await db.Tasks
    .Where(t => t.Comments.Count() > 2)
    .Select(t => new { t.Id, t.Title, Comments = t.Comments.Count() })
    .OrderByDescending(x => x.Comments)
    .ToListAsync();
q2.ForEach(x => Console.WriteLine($"[{x.Id}] {x.Title} — {x.Comments} comments"));

// 3) User who created the most tasks with BUG tag
H("3) User with most BUG tasks created");
var q3 = await db.Tasks
    .Where(t => t.Tags.Any(tag => tag.Name == "BUG"))
    .GroupBy(t => t.Creator)
    .Select(g => new { User = g.Key, Count = g.Count() })
    .OrderByDescending(x => x.Count)
    .FirstOrDefaultAsync();
Console.WriteLine(q3 is null ? "No BUG tasks" : $"{q3.User.Name} ({q3.User.Email}) — {q3.Count} BUG tasks");

// 4) Count the number of tasks for each tag
H("4) Tasks count per tag");
var q4 = await db.Tags
    .Select(tag => new
    {
        tag.Name,
        TasksCount = tag.Tasks.Select(t => t.Id).Distinct().Count()
    })
    .OrderByDescending(x => x.TasksCount)
    .ToListAsync();
q4.ForEach(x => Console.WriteLine($"{x.Name}: {x.TasksCount}"));

// 5) Tasks where creator == assignee
H("5) Tasks where creator == assignee");
var q5 = await db.Tasks
    .Where(t => t.AssigneeId != null && t.CreatorId == t.AssigneeId)
    .Select(t => new { t.Id, t.Title })
    .ToListAsync();
q5.ForEach(x => Console.WriteLine($"[{x.Id}] {x.Title}"));

// 6) Latest comment for each task (first 15 tasks with comments)
H("6) Latest comment per task (first 15)");
var q6 = await db.Tasks
    .Where(t => t.Comments.Any())
    .Select(t => new
    {
        t.Id,
        t.Title,
        Latest = t.Comments
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new { c.Id, c.Text, c.CreatedAt })
            .FirstOrDefault()
    })
    .OrderBy(t => t.Id)
    .Take(15)
    .ToListAsync();
q6.ForEach(x => Console.WriteLine($"[{x.Id}] {x.Title} -> c#{x.Latest!.Id} at {x.Latest.CreatedAt:u}: {x.Latest.Text}"));

// 7) Tasks that have more than one tag
H("7) Tasks with >1 tag");
var q7 = await db.Tasks
    .Where(t => t.Tags.Count() > 1)
    .Select(t => new { t.Id, t.Title, TagCount = t.Tags.Count() })
    .OrderByDescending(x => x.TagCount)
    .ToListAsync();
q7.ForEach(x => Console.WriteLine($"[{x.Id}] {x.Title} — {x.TagCount} tags"));

// 8) Total number of comments per user (desc)
H("8) Comments per user (desc)");
var q8 = await db.Users
    .Select(u => new
    {
        u.Id,
        u.Name,
        Count = db.Set<Comment>().Count(c => c.AuthorId == u.Id)
    })
    .OrderByDescending(x => x.Count)
    .ToListAsync();
q8.ForEach(x => Console.WriteLine($"{x.Name}: {x.Count}"));

// 9) Rank teams by number of created and assigned tasks
H("9) Teams ranked by created+assigned");
var q9 = await db.Teams
    .Select(team => new
    {
        Team = team,
        Created = team.Members.SelectMany(m => m.CreatedTasks).Select(t => t.Id).Distinct().Count(),
        Assigned = team.Members.SelectMany(m => m.AssignedTasks).Select(t => t.Id).Distinct().Count()
    })
    .OrderByDescending(x => x.Created + x.Assigned)
    .ToListAsync();
q9.ForEach(x => Console.WriteLine($"{x.Team.Name}: created={x.Created}, assigned={x.Assigned}, total={x.Created + x.Assigned}"));

// 10) Users who left comments under a task with tag STORY (Name, Email, Task title, Project name, Team name)
H("10) Commenters under STORY tasks");
var q10 = await db.Set<Comment>()
    .Where(c => c.Task.Tags.Any(tg => tg.Name == "STORY"))
    .SelectMany(c => c.Author.Teams.DefaultIfEmpty(), (c, team) => new
    {
        UserName = c.Author.Name,
        UserEmail = c.Author.Email,
        TaskTitle = c.Task.Title,
        ProjectName = c.Task.Project.Name,
        TeamName = team != null ? team.Name : null
    })
    .Distinct()
    .ToListAsync();
q10.ForEach(x => Console.WriteLine($"{x.UserName} <{x.UserEmail}> | Task: {x.TaskTitle} | Project: {x.ProjectName} | Team: {x.TeamName}"));

// 11) For each user, the most frequently used tag on tasks they created
H("11) User → most used tag on their created tasks");
var q11 = await db.Users
    .Select(u => new
    {
        u.Name,
        TopTag = u.CreatedTasks
            .SelectMany(t => t.Tags)
            .GroupBy(tag => tag.Name)
            .Select(g => new { Tag = g.Key, C = g.Count() })
            .OrderByDescending(x => x.C)
            .FirstOrDefault()
    })
    .ToListAsync();
q11.ForEach(x => Console.WriteLine($"{x.Name}: {(x.TopTag == null ? "-" : $"{x.TopTag.Tag} ({x.TopTag.C})")}"));


// 12) Projects ordered by the average number of comments per task
H("12) Projects by avg comments per task");
var q12 = await db.Projects
    .Select(p => new
    {
        p.Name,
        Avg = p.Tasks.Any() ? p.Tasks.Average(t => t.Comments.Count) : 0
    })
    .OrderByDescending(x => x.Avg)
    .ToListAsync();
q12.ForEach(x => Console.WriteLine($"{x.Name}: avg comments = {x.Avg:F2}"));

// 13) Users who have commented on tasks they did not create
H("13) Users who commented on tasks they did not create");
var q13 = await db.Set<Comment>()
    .Where(c => c.AuthorId != c.Task.CreatorId)
    .Select(c => c.Author)
    .Distinct()
    .Select(u => new { u.Id, u.Name, u.Email })
    .ToListAsync();
q13.ForEach(x => Console.WriteLine($"{x.Name} <{x.Email}>"));

Console.WriteLine();
Console.WriteLine("Done.");
