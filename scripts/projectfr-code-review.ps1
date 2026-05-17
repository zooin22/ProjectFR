param(
    [string]$RepoPath = (Get-Location).Path,
    [string]$Branch = "main",
    [string]$Target = "7804175457",
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$utf8NoBom = [System.Text.UTF8Encoding]::new($false)
[Console]::InputEncoding = $utf8NoBom
[Console]::OutputEncoding = $utf8NoBom
$OutputEncoding = $utf8NoBom
$PSDefaultParameterValues['Out-File:Encoding'] = 'utf8'
$PSDefaultParameterValues['Set-Content:Encoding'] = 'utf8'
$env:OPENAI_API_ENCODING = 'utf-8'
$env:PYTHONIOENCODING = 'utf-8'
$env:LANG = 'ko_KR.UTF-8'
$env:LC_ALL = 'ko_KR.UTF-8'

function ConvertTo-ArgumentString {
    param([string[]]$ArgumentList)

    return ($ArgumentList | ForEach-Object {
        if ($_ -match '[\s"]') {
            '"' + ($_ -replace '"', '\"') + '"'
        }
        else {
            $_
        }
    }) -join ' '
}

function Invoke-Utf8Process {
    param(
        [Parameter(Mandatory = $true)][string]$FilePath,
        [string[]]$ArgumentList = @(),
        [string]$WorkingDirectory = (Get-Location).Path,
        [hashtable]$Environment = @{}
    )

    $startInfo = New-Object System.Diagnostics.ProcessStartInfo
    $startInfo.FileName = $FilePath
    $startInfo.Arguments = ConvertTo-ArgumentString $ArgumentList
    $startInfo.WorkingDirectory = $WorkingDirectory
    $startInfo.UseShellExecute = $false
    $startInfo.RedirectStandardOutput = $true
    $startInfo.RedirectStandardError = $true
    $startInfo.StandardOutputEncoding = $utf8NoBom
    $startInfo.StandardErrorEncoding = $utf8NoBom

    foreach ($entry in $Environment.GetEnumerator()) {
        $startInfo.EnvironmentVariables[$entry.Key] = [string]$entry.Value
    }

    $process = New-Object System.Diagnostics.Process
    $process.StartInfo = $startInfo
    [void]$process.Start()

    $stdout = $process.StandardOutput.ReadToEnd()
    $stderr = $process.StandardError.ReadToEnd()
    $process.WaitForExit()

    [pscustomobject]@{
        ExitCode = $process.ExitCode
        StdOut = $stdout
        StdErr = $stderr
    }
}

Set-Location $RepoPath

$changedFiles = git diff --name-only "origin/$Branch..HEAD" 2>$null
if (-not $changedFiles) {
    $changedFiles = git diff --name-only HEAD~1..HEAD 2>$null
}

$changedFiles = @($changedFiles | Where-Object { $_ -and $_.Trim().Length -gt 0 })
$changedSummary = if ($changedFiles.Count -gt 0) { ($changedFiles -join ', ') } else { '변경 파일 감지 실패 또는 없음' }

$prompt = @"
다음 저장소의 최근 변경사항을 리뷰하고, 아키텍처 관점, C# 컨벤션, 잠재적 버그를 간단히 짚어줘.
저장소: $RepoPath
브랜치: $Branch
변경 파일: $changedSummary
결과는 한국어로, 텔레그램에 보내기 좋게 bullet 위주로 작성해줘.
"@

$commonEnv = @{
    OPENAI_API_ENCODING = 'utf-8'
    PYTHONIOENCODING = 'utf-8'
    LANG = 'ko_KR.UTF-8'
    LC_ALL = 'ko_KR.UTF-8'
}

$claudeResult = Invoke-Utf8Process -FilePath 'C:\Users\zooin\AppData\Roaming\npm\claude.cmd' -ArgumentList @('-p', $prompt) -WorkingDirectory $RepoPath -Environment $commonEnv
$review = ($claudeResult.StdOut + $claudeResult.StdErr).Trim() -replace "`r`n", "`n"
if (-not $review) {
    $review = '코드 리뷰 결과가 비어 있음.'
}

$message = @"
ProjectFR 코드 리뷰 완료
브랜치: $Branch
변경 파일: $changedSummary

$review
"@

if ($message.Length -gt 3500) {
    $message = $message.Substring(0, 3500) + "`n... (이하 생략)"
}

if ($DryRun) {
    Write-Output $message
    exit 0
}

$sendResult = Invoke-Utf8Process -FilePath 'C:\Users\zooin\AppData\Roaming\npm\openclaw.cmd' -ArgumentList @('message', 'send', '--channel', 'telegram', '--target', $Target, '--message', $message, '--silent') -WorkingDirectory $RepoPath -Environment $commonEnv
if ($sendResult.ExitCode -ne 0) {
    throw "메시지 전송 실패: $($sendResult.StdErr)$($sendResult.StdOut)"
}
