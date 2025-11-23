# PowerToys 扩展开发 - 快速更新指南

## 🔥 热重载模式 (最快!)

启用 PowerToys 设置 → 适用于开发人员 → "启用外部重新加载" 后:

```powershell
.\hot-reload.ps1
```

**优势:**
- ⚡ 无需关闭/重启 PowerToys
- ⏱️ 更新时间从 15 秒减少到 8 秒
- 🎯 使用 `x-cmdpal://reload` 命令即时重载

### 使用 VS Code (推荐)
按 `Ctrl+Shift+B` → 自动运行 **"🔥 热重载扩展"** 任务

---

## 🚀 三种更新方式

### 方式 1: 完整更新流程 (推荐)
```powershell
.\update-extension.ps1
```
自动完成:
- ✓ 关闭 PowerToys
- ✓ 卸载旧版本
- ✓ 重新构建项目
- ✓ 安装新版本
- ✓ 重启 PowerToys

### 方式 2: 使用 VS Code 任务
1. 按 `Ctrl+Shift+B` 或 `Ctrl+Shift+P` → "Run Task"
2. 选择 **"🔄 更新扩展 (完整流程)"**

这是最快的方式,设为默认构建任务!

### 方式 3: 手动分步更新
```powershell
# 1. 快速检查编译错误
.\quick-rebuild.ps1

# 2. 如果构建成功,跳过重复构建直接安装
.\update-extension.ps1 -SkipBuild
```

## 📝 开发工作流

### ⚡ 热重载工作流 (推荐):
```
1. 修改代码
2. 按 Ctrl+Shift+B (自动运行热重载)
3. 等待 8-10 秒
4. 按 Alt+Space 立即测试 - PowerToys 保持运行!
```

### 传统工作流:
```
1. 修改代码 (ProjectOpenerExtensionPage.cs 等)
2. 按 Ctrl+Shift+B (运行完整更新流程)
3. 等待 10-15 秒完成更新
4. 按 Alt+Space 测试更改
```

### 快速测试模式:
如果只想检查语法错误,不想安装:
```powershell
.\quick-rebuild.ps1
```

## ⚡ 性能提示

### 跳过重启 PowerToys
如果您想手动控制 PowerToys:
```powershell
.\update-extension.ps1 -SkipRestart
```

### 构建 + 安装分离
```powershell
# 先构建包含 MSIX
dotnet build -c Debug -r win-x64 /p:Platform=x64 /p:GenerateAppxPackageOnBuild=true

# 然后快速安装(跳过构建)
.\update-extension.ps1 -SkipBuild
```

## 🐛 调试技巧

### 查看应用日志
```powershell
# 如果扩展崩溃或有问题,查看详细日志
Get-AppxPackage -Name "ProjectOpenerExtension" | Select-Object *
```

### 完全清理重装
```powershell
# 1. 卸载
Remove-AppxPackage -Package "ProjectOpenerExtension_0.0.1.0_x64__8wekyb3d8bbwe"

# 2. 清理构建输出
Remove-Item -Recurse -Force .\ProjectOpenerExtension\bin, .\ProjectOpenerExtension\obj

# 3. 重新构建安装
.\update-extension.ps1
```

### PowerToys 命令面板调试
- PowerToys 设置 → Command Palette → 查看已安装的扩展
- 检查扩展是否出现在列表中
- 尝试禁用/启用扩展

## 🔑 快捷键

| 快捷键         | 功能                        |
| -------------- | --------------------------- |
| `Ctrl+Shift+B` | VS Code 快速构建和更新      |
| `Alt+Space`    | 打开 PowerToys 命令面板     |
| `Ctrl+Shift+P` | VS Code 命令面板 → Run Task |

## 📦 文件位置

```
ProjectOpenerExtension/
├── hot-reload.ps1                 # 🔥 热重载脚本 (推荐) ⭐⭐⭐
├── update-extension.ps1           # 完整更新脚本 ⭐
├── quick-rebuild.ps1              # 快速构建脚本
├── .vscode/tasks.json             # VS Code 任务配置
├── ProjectOpenerExtension/
│   ├── ProjectOpenerExtensionCommandsProvider.cs  # 命令提供者
│   ├── Pages/ProjectOpenerExtensionPage.cs        # 页面实现
│   └── AppPackages/.../_Test/*.msix               # 安装包
```

## 🎯 常见问题

**Q: 热重载和完整更新有什么区别?**
A: 
- **热重载**: 不关闭 PowerToys,使用 `x-cmdpal://reload` 重载扩展,速度更快
- **完整更新**: 关闭并重启 PowerToys,适合首次安装或热重载失败时使用

**Q: 修改代码后扩展没有更新?**
A: 
1. 先尝试热重载: `.\hot-reload.ps1`
2. 如果不行,使用完整更新: `.\update-extension.ps1`
3. 确认已启用 PowerToys 的 "启用外部重新加载" 选项

**Q: 热重载不工作?**
A:
1. 确认 PowerToys 设置中已启用 "启用外部重新加载"
2. 使用完整更新脚本: `.\update-extension.ps1`
3. 检查扩展是否正确安装: `Get-AppxPackage -Name "ProjectOpenerExtension"`

**Q: 更新后 PowerToys 命令面板找不到扩展?**
A: 
1. 检查扩展是否安装: `Get-AppxPackage -Name "ProjectOpenerExtension"`
2. 重启 PowerToys
3. 检查 PowerToys 设置中是否启用了 Command Palette 模块

**Q: 提示证书错误?**
A: 以管理员身份运行:
```powershell
Import-Certificate -FilePath "C:\code\CSharp\ProjectOpenerExtension\ProjectOpenerExtension.cer" -CertStoreLocation "Cert:\LocalMachine\Root"
```

**Q: 想改版本号?**
A: 修改 `Package.appxmanifest` 中的 Version,然后更新脚本中的包名。

---

💡 **终极提示**: 
- 启用 "外部重新加载" 后,`Ctrl+Shift+B` 自动运行热重载
- 每次修改代码只需 8 秒即可测试!
- 不再需要等待 PowerToys 重启 🚀
