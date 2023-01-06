using System.Threading.Tasks;

namespace Tools;

public interface IAction
{
    Task Do();
}