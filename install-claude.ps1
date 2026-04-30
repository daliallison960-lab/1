# install-claude.ps1 — One-click installer for Claude Code CLI (official) on Windows.
# Run in PowerShell:  irm https://your-host/install-claude.ps1 | iex
# or:  powershell -ExecutionPolicy Bypass -File .\install-claude.ps1

$ErrorActionPreference = 'Stop'

function Info($m) { Write-Host "[i] $m" -ForegroundColor Cyan }
function Ok($m)   { Write-Host "[√] $m" -ForegroundColor Green }
function Warn($m) { Write-Host "[!] $m" -ForegroundColor Yellow }
function Die($m)  { Write-Host "[x] $m" -ForegroundColor Red; exit 1 }

Info "Detected platform: Windows"

if (Get-Command claude -ErrorAction SilentlyContinue) {
    $v = (claude --version) 2>$null
    Ok "Claude Code already installed: $v"
} else {
    Info "Installing Claude Code CLI…"
    try {
        Invoke-RestMethod https://claude.ai/install.ps1 | Invoke-Expression
    } catch {
        Die "Installer failed: $_"
    }

    # Refresh PATH for this session.
    $userPath = [Environment]::GetEnvironmentVariable('Path','User')
    $machinePath = [Environment]::GetEnvironmentVariable('Path','Machine')
    $env:Path = "$machinePath;$userPath"

    if (-not (Get-Command claude -ErrorAction SilentlyContinue)) {
        Die "Install finished but 'claude' not on PATH. Open a new PowerShell window and re-run."
    }
    Ok "Installed: $(claude --version)"
}

Write-Host ""
Write-Host "Next: a browser window will open for you to sign in." -ForegroundColor Green
Write-Host "  - Choose 'Claude account' for Pro/Max subscription."
Write-Host "  - Choose 'Anthropic Console' for pay-per-use API billing."
Write-Host ""
Read-Host "Press Enter to launch 'claude' now (Ctrl+C to skip)"
& claude
