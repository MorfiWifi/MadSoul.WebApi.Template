using System.Reflection;
using MadSoul.AspCommon;

namespace Products.HttpServer;

public static class Helper
{
    public static Assembly GetAssembly()
        => Assembly.GetExecutingAssembly();
}