using System.Reflection;
namespace PostsAPI.Services;

public class MiniContainer
{
    private readonly Dictionary<Type, Type> _recipes = new();
    private readonly Dictionary<Type, object> _singletons = new();

    public void Register<TService, TImplementation>() where TImplementation : TService
        => _recipes[typeof(TService)] = typeof(TImplementation);

    public T Resolve<T>() => (T)Resolve(typeof(T));
    public T ResolveSingleton<T>() => (T)ResolveSingleton(typeof(T));

    private object ResolveSingleton(Type type)
    {
        if (_singletons.ContainsKey(type)) return _singletons[type];
        Type t = _recipes.ContainsKey(type) ? _recipes[type] : type;
        ConstructorInfo ctor = t.GetConstructors()[0];
        ParameterInfo[] parameters = ctor.GetParameters();
        List<object> args = new();
        foreach (var p in parameters)
        {
            args.Add(ResolveSingleton(p.ParameterType));
        }
        object newInstance = ctor.Invoke(args.ToArray());
        _singletons[type] = newInstance;
        return newInstance;
    }
    private object Resolve(Type type)
    {
        Type t = _recipes.ContainsKey(type) ? _recipes[type] : type;
        ConstructorInfo ctor = t.GetConstructors()[0];
        ParameterInfo[] parameters = ctor.GetParameters();
        List<object> args = new();
        foreach(var p in parameters)
        {
            args.Add(Resolve(p.ParameterType));
        }
        return ctor.Invoke(args.ToArray());
    }
}

public interface IEmailSender { void Send(string to); }
public class FakeEmailSender : IEmailSender {
    private static int _createdSenders = 0;
    private int Id { get; set; }

    public FakeEmailSender()
    {
        Id = ++_createdSenders;
    }
    public void Send(string to) => Console.WriteLine($"Sender of ID [{Id}] sending to {to}");
}
public class OrderService
{
    private static int _createdServices = 0;

    private int Id { get; set; }
    private readonly IEmailSender _sender;
    public OrderService(IEmailSender sender) {
        _sender = sender;
        Id = ++_createdServices;
    }
    public void Place()
    {
        Console.WriteLine($"Sending from service of ID [{Id}]");
        _sender.Send("customer@example.com");
    }
}
