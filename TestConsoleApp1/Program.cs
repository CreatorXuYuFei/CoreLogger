using CoreLogger;
using CoreLogger.Enum;
using Microsoft.Extensions.Logging;

namespace TestConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            // ==============================================
            // 日志工具全局配置：开启所有核心功能
            // ==============================================
            SysLogHelper.Configure(opt =>
            {
                // ================== 全局配置 ==================
                opt.GlobalMinLevel = LogLevel.Debug;    // 全局最低日志级别（Debug及以上全部记录）
                opt.SampleRate = 1.0;                   // 日志采样率（1.0=100%记录）
                opt.TextTemplate = "{UtcTimestamp:yyyy-MM-dd HH:mm:ss} [{ThreadId}] [{Level}] [{Module}] {Scope} {Message}"; // 日志模板

                // ================== 控制台输出（开启） ==================
                opt.Console.Enabled = true;                // 启用控制台打印
                opt.Console.MinLevel = LogLevel.Information;     // 控制台最低级别
                opt.Console.Format = LogFormatType.Text;  // 输出格式：Text=文本 / Json=JSON

                // ================== 文件输出（核心配置） ==================
                opt.File.Enabled = true;                  // 启用文件日志
                opt.File.RootPath = Path.Combine(AppContext.BaseDirectory, "my_logs"); // 自定义存储目录
                opt.File.FileName = "app_runtime";        // 自定义日志文件名（无后缀）
                opt.File.RollingInterval = RollingInterval.Day; // 按天滚动文件
                opt.File.MaxFileSizeMB = 10;              // 单个文件最大10MB
                opt.File.MaxBackupCount = 30;             // 最多保留30个历史文件
                opt.File.EnableCompress = true;           // 自动压缩旧日志
                opt.File.Format = LogFormatType.Json;     // 文件日志格式

                // ================== 敏感信息脱敏（自动隐藏手机号/身份证） ==================
                opt.SensitiveMask.Enabled = true;
                // 新增自定义脱敏规则（邮箱）
                opt.SensitiveMask.Rules.Add(@"[\w]+@[\w]+\.[\w]+", "***@mail.com");

                // ================== 日志过滤 ==================
                opt.Filter.Enabled = false;
                opt.Filter.IncludeModules.Add("User");    // 只包含 User 模块
                opt.Filter.ExcludeModules.Add("System");  // 排除 System 模块
                opt.Filter.BlockKeywords.Add("测试密码");  // 屏蔽包含该关键词的日志
            });

            // 写日志
            SysLogHelper.Info("日志已保存为：backend_service_log.log");
            SysLogHelper.Error("测试错误日志", new Exception("测试"));

            // ==============================================
            // 🔥 2. 全级别日志测试
            // ==============================================
            SysLogHelper.Debug("调试日志：系统初始化完成");
            SysLogHelper.Info("信息日志：用户登录成功");
            SysLogHelper.Warn("警告日志：磁盘空间不足");
            SysLogHelper.Error("错误日志：接口请求失败", new Exception("网络连接超时"));
            SysLogHelper.Critical("致命错误：服务崩溃", new Exception("内存溢出"));

            // ==============================================
            // 🔥 3. 敏感信息脱敏测试（自动隐藏）
            // ==============================================
            SysLogHelper.Info("用户信息：手机号13812345678，身份证110101199003074567，邮箱test@163.com");

            // ==============================================
            // 🔥 4. 日志作用域（上下文追踪：TraceId/用户ID）
            // ==============================================
            using (SysLogHelper.BeginScope(new { TraceId = Guid.NewGuid(), UserId = 1001, Operation = "支付" }))
            {
                SysLogHelper.Info("订单创建成功", "Order"); // 日志自动携带上下文信息
            }

            // ==============================================
            // 🔥 5. 多模块分类日志
            // ==============================================
            SysLogHelper.Info("用户模块日志", "User");
            SysLogHelper.Info("订单模块日志", "Order");

            Console.WriteLine("所有日志功能测试完成！");
            Console.ReadLine();
        }
    }
}
