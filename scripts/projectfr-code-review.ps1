param(
    [string]$RepoPath = (Get-Location).Path,
    [string]$Branch = "main",
    [string]$Target = "7804175457",
    [string]$ReviewText = "",
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

try { chcp 65001 > $null } catch {}

$claudePath = 'C:\Users\zooin\AppData\Roaming\npm\claude.cmd'
$openclawPath = 'C:\Users\zooin\AppData\Roaming\npm\openclaw.cmd'

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

if ($ReviewText) {
    $review = $ReviewText
}
else {
    $review = (& $claudePath -p $prompt 2>&1 | Out-String).Trim()
    $review = $review -replace "`r`n", "`n"
    if (-not $review) {
        $review = '코드 리뷰 결과가 비어 있음.'
    }
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

& $openclawPath message send --channel telegram --target $Target --message $message --silent | Out-Null
if ($LASTEXITCODE -ne 0) {
    throw '메시지 전송 실패'
}
