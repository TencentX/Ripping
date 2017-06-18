using UnityEngine;
using System.Collections;

// 让每一个业务模块都有一个统一的管理

//public class Singleton<T> : IDisposable where T : new()
interface Module{

    // 由系统调用
    // 游戏启动就初始化
    // 在回到登录界面之前Release，接着到了登录界面再次Init
    void Init();

    // 由系统调用
    // 回登录界面之前Release，到了登录界面接着Init
    void Release();

    // 表示启动模块，一些带UI的模块，调用后应该UI也跟着打开
    // 由业务自己决定调用时机    
    void Entry();

    // 表示退出模块
    // 由业务自己决定调用时机，登出时会调用一次才Release
    // 应该清理干净自己模块的运行时开销
    void Exit();
}
