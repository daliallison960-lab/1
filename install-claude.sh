#!/usr/bin/env bash
# install-claude.sh — One-click installer for Claude Code CLI (official).
# Supports: macOS, Linux, WSL. For Windows native, see install-claude.ps1.

set -euo pipefail

GREEN='\033[0;32m'; YELLOW='\033[1;33m'; RED='\033[0;31m'; BLUE='\033[0;34m'; NC='\033[0m'
info()  { printf "${BLUE}[i]${NC} %s\n" "$*"; }
ok()    { printf "${GREEN}[✓]${NC} %s\n" "$*"; }
warn()  { printf "${YELLOW}[!]${NC} %s\n" "$*"; }
die()   { printf "${RED}[x]${NC} %s\n" "$*" >&2; exit 1; }

OS="$(uname -s)"
case "$OS" in
  Darwin) PLATFORM="macOS" ;;
  Linux)
    if grep -qi microsoft /proc/version 2>/dev/null; then PLATFORM="WSL"
    else PLATFORM="Linux"; fi ;;
  *) die "Unsupported OS: $OS. Use install-claude.ps1 on Windows." ;;
esac
info "Detected platform: $PLATFORM"

if command -v claude >/dev/null 2>&1; then
  CURRENT="$(claude --version 2>/dev/null || echo unknown)"
  ok "Claude Code already installed: $CURRENT"
else
  info "Installing Claude Code CLI…"
  if command -v curl >/dev/null 2>&1; then
    curl -fsSL https://claude.ai/install.sh | bash
  elif command -v wget >/dev/null 2>&1; then
    wget -qO- https://claude.ai/install.sh | bash
  else
    die "Need curl or wget to download installer."
  fi

  # Make sure the install dir is on PATH for this session.
  for d in "$HOME/.local/bin" "$HOME/.claude/bin" "/usr/local/bin"; do
    [ -d "$d" ] && case ":$PATH:" in *":$d:"*) ;; *) export PATH="$d:$PATH" ;; esac
  done

  command -v claude >/dev/null 2>&1 || die "Install finished but 'claude' not on PATH. Open a new terminal and re-run."
  ok "Installed: $(claude --version)"
fi

cat <<EOF

${GREEN}Next:${NC} a browser window will open for you to sign in.
  • Choose ${BLUE}Claude account${NC} for Pro/Max subscription.
  • Choose ${BLUE}Anthropic Console${NC} for pay-per-use API billing.

Press Enter to launch \`claude\` now (Ctrl+C to skip)…
EOF
read -r _ || true
exec claude
