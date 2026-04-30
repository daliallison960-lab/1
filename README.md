# Claude Code 一键安装脚本

在你**自己的本地终端**运行，自动安装 Claude Code CLI 并启动登录流程（OAuth 走浏览器，登录到 Anthropic 官服）。

## macOS / Linux / WSL

```bash
curl -fsSL https://raw.githubusercontent.com/daliallison960-lab/1/claude/initial-setup-jlb3G/install-claude.sh -o install-claude.sh
chmod +x install-claude.sh
./install-claude.sh
```

或者本地下载后直接执行：

```bash
bash install-claude.sh
```

## Windows (PowerShell)

```powershell
irm https://raw.githubusercontent.com/daliallison960-lab/1/claude/initial-setup-jlb3G/install-claude.ps1 -OutFile install-claude.ps1
powershell -ExecutionPolicy Bypass -File .\install-claude.ps1
```

## 脚本做了什么

1. 检测操作系统（macOS / Linux / WSL / Windows）
2. 如未安装，调用官方安装器：
   - Unix: `curl -fsSL https://claude.ai/install.sh | bash`
   - Windows: `irm https://claude.ai/install.ps1 | iex`
3. 把安装路径加进当前会话的 `PATH`
4. 启动 `claude`，提示在浏览器完成 OAuth 登录
   - **Claude account** — Pro/Max 订阅
   - **Anthropic Console** — 按 API 用量计费

## 验证

```bash
claude --version
claude /status
```

## 常用命令

- `claude` — 启动交互会话
- `claude -p "..."` — 一次性非交互输出
- `/login` / `/logout` — 切换账号
- `claude --help` — 看全部参数
