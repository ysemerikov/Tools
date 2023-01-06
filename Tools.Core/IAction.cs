using System.Threading.Tasks;

namespace Tools.Core;

public interface IAction
{
    Task Do();
}