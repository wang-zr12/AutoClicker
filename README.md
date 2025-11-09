AutoClicker
=== 自动点击工具使用说明 (C# 版本) ===

【编译方法】
Windows系统:
  双击运行 build.bat
  无需安装Python或任何依赖
  使用系统自带的 .NET Framework 编译器

Linux/Mac系统:
  终端运行: ./build.sh
  需要安装 Mono: sudo apt-get install mono-complete

编译完成后，AutoClicker.exe 将直接生成在当前目录。

【使用说明】
1. 双击运行 AutoClicker.exe（Windows）
2. 在输入框输入每秒点击次数（1-500）
3. 按 F8 开始/停止自动点击
4. 按 ESC 退出程序
5. 红色方块 = 停止状态，绿色方块 = 运行状态

【优势】
- 纯C#原生代码，性能更好
- 编译后单个EXE文件，无需依赖
- 文件体积更小（约10KB）
- 启动速度更快
- 资源占用更低

【技术特性】
- 使用 Windows Forms UI
- 直接调用 Win32 API (user32.dll)
- 多线程点击循环
- 全局热键支持（F8）
- 窗口拖动功能

【注意事项】
- 窗口可拖动
- 窗口始终置顶
- 仅支持 Windows 系统
- 需要 .NET Framework 4.0 或更高版本（Windows 7+ 自带）
