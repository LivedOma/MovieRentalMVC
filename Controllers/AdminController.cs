using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MovieRental.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<AdminController> _logger;

    public AdminController(IWebHostEnvironment environment, ILogger<AdminController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    // GET: Admin/Logs
    public IActionResult Logs(int lines = 100)
    {
        var logsPath = Path.Combine(_environment.ContentRootPath, "Logs");
        var logFiles = new List<LogFileViewModel>();

        if (Directory.Exists(logsPath))
        {
            var files = Directory.GetFiles(logsPath, "*.txt")
                .OrderByDescending(f => System.IO.File.GetLastWriteTime(f))
                .Take(7); // Últimos 7 días

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                logFiles.Add(new LogFileViewModel
                {
                    FileName = fileInfo.Name,
                    FileSize = FormatFileSize(fileInfo.Length),
                    LastModified = fileInfo.LastWriteTime
                });
            }
        }

        // Leer el log más reciente
        var latestLog = logFiles.FirstOrDefault();
        var logContent = string.Empty;

        if (latestLog != null)
        {
            var latestLogPath = Path.Combine(logsPath, latestLog.FileName);
            try
            {
                // Leer las últimas N líneas
                var allLines = System.IO.File.ReadAllLines(latestLogPath);
                var lastLines = allLines.TakeLast(lines);
                logContent = string.Join(Environment.NewLine, lastLines);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading log file: {FileName}", latestLog.FileName);
                logContent = $"Error reading log file: {ex.Message}";
            }
        }

        var viewModel = new LogsViewModel
        {
            LogFiles = logFiles,
            CurrentLogContent = logContent,
            LinesShown = lines
        };

        return View(viewModel);
    }

    // GET: Admin/DownloadLog
    public IActionResult DownloadLog(string fileName)
    {
        if (string.IsNullOrEmpty(fileName) || fileName.Contains(".."))
        {
            return BadRequest("Invalid file name");
        }

        var logsPath = Path.Combine(_environment.ContentRootPath, "Logs");
        var filePath = Path.Combine(logsPath, fileName);

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound();
        }

        _logger.LogInformation("Admin downloaded log file: {FileName}", fileName);

        var fileBytes = System.IO.File.ReadAllBytes(filePath);
        return File(fileBytes, "text/plain", fileName);
    }

    private static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }
}

public class LogsViewModel
{
    public List<LogFileViewModel> LogFiles { get; set; } = new();
    public string CurrentLogContent { get; set; } = string.Empty;
    public int LinesShown { get; set; }
}

public class LogFileViewModel
{
    public string FileName { get; set; } = string.Empty;
    public string FileSize { get; set; } = string.Empty;
    public DateTime LastModified { get; set; }
}