using System.Runtime.InteropServices;
using BenchmarkDotNet.Attributes;
using Bogus;
using FastListIteration.Models;

namespace FastListIteration;

[MemoryDiagnoser(false)]
public class Benchmarks {

    [Params(10, 100, 1000)]
    public int Size { get; set; }

    private IEnumerable<Order> _orderEnumerable;
    private List<Order> _orderList = new();

    public static Faker<BillingDetails> BillingDetailsFaker = new Faker<BillingDetails>()
        .RuleFor(x => x.CustomerName, x => x.Person.FullName)
        .RuleFor(x => x.Email, x => x.Person.Email)
        .RuleFor(x => x.Phone, x => x.Person.Phone)
        .RuleFor(x => x.AddressLine, x => x.Address.StreetAddress())
        .RuleFor(x => x.City, x => x.Address.City())
        .RuleFor(x => x.PostCode, x => x.Address.ZipCode())
        .RuleFor(x => x.Country, x => x.Address.Country());

    public static Faker<Order> OrderFaker = new Faker<Order>()
        .RuleFor(x => x.Id, x => Guid.NewGuid())
        .RuleFor(x => x.Price, x => x.Finance.Amount(5, 100))
        .RuleFor(x => x.Currency, x => x.Finance.Currency().Code)
        .RuleFor(x => x.BillingDetails, x => BillingDetailsFaker);

    [GlobalSetup]
    public void Setup() {

        Randomizer.Seed = new Random(54815148);
        _orderEnumerable = OrderFaker.Generate(Size); // as IEnumerable<Order>;
        _orderList = _orderEnumerable.ToList();
    }

    [Benchmark]
    public void For_Enumerable() {
        for (var i = 0; i < _orderEnumerable.Count(); i++) {
            var order = _orderEnumerable.ToList()[i];
            DoSomething(order);
        }
    }

    [Benchmark]
    public void For_List() {
        for (var i = 0; i < _orderList.Count; i++) {
            var order = _orderList[i];
            DoSomething(order);
        }
    }

    [Benchmark]
    public void Foreach_Enumerable() {
        foreach (var order in _orderEnumerable) {
            DoSomething(order);
        }
    }

    [Benchmark]
    public void Foreach_List() {
        foreach (var order in _orderList) {
            DoSomething(order);
        }
    }

    [Benchmark]
    public void Foreach_Enumerable_Linq() {
        _orderEnumerable.ToList().ForEach(DoSomething);
    }

    [Benchmark]
    public void Foreach_List_Linq() {
        _orderList.ForEach(DoSomething);
    }

    [Benchmark]
    public void Parallel_Enumerable_Foreach() {
        Parallel.ForEach(_orderEnumerable, DoSomething);
    }

    [Benchmark]
    public void Parallel_List_Foreach() {
        Parallel.ForEach(_orderList, DoSomething);
    }

    [Benchmark]
    public void Parallel_Enumerable_Linq() {
        _orderEnumerable.AsParallel().ForAll(DoSomething);
    }

    [Benchmark]
    public void Parallel_List_Linq() {
        _orderList.AsParallel().ForAll(DoSomething);
    }

    [Benchmark]
    public void ForEach_Enumerable_Span() {
        foreach (var order in CollectionsMarshal.AsSpan(_orderEnumerable.ToList())) {
            DoSomething(order);
        }
    }

    [Benchmark]
    public void ForEach_List_Span() {
        foreach (var order in CollectionsMarshal.AsSpan(_orderList)) {
            DoSomething(order);
        }
    }

    [Benchmark]
    public void For_Enumerable_Span() {
        var asSpan = CollectionsMarshal.AsSpan(_orderEnumerable.ToList());
        for (var i = 0; i < asSpan.Length; i++) {
            var order = asSpan[i];
            DoSomething(order);
        }
    }

    [Benchmark]
    public void For_List_Span() {
        var asSpan = CollectionsMarshal.AsSpan(_orderList.ToList());
        for (var i = 0; i < asSpan.Length; i++) {
            var order = asSpan[i];
            DoSomething(order);
        }
    }

    public static void DoSomething(Order order) {
        //Console.WriteLine(order.BillingDetails.CustomerName);
    }
}