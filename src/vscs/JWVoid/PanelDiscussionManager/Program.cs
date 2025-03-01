using PanelDiscussionManager.Application;
using PanelDiscussionManager.Domain.Entities;
using PanelDiscussionManager.Domain.Services;
using PanelDiscussionManager.Infrastructure.Interfaces;
using PanelDiscussionManager.Infrastructure.Services;



var persons = new HashSet<Person>([
    new Person("L"),
    new Person("G"),
    //new Person("S"),
    //new Person("C"),
    //new Person("F"),
    new Person("A")
]);
var questions = new Question[] {
    new Question("1", "a?", new TimeSpan(0, 0, 30)),
    new Question("1", "b?", new TimeSpan(0, 0, 30)),
    new Question("5", "j?", new TimeSpan(0, 0, 30))
};



DiscussionServer discussionServer;
{
    var mathService = new MathService();
    var randomService = new RandomService();
    var shuffleService = new ShuffleService(randomService);
    var discussantOrderCalculatorService = new DiscussantOrderRandomCalculatorService(mathService, randomService, shuffleService);
    foreach (var service in new IService[] { mathService, randomService, shuffleService, discussantOrderCalculatorService })
    {
        service.Init();
    }

    discussionServer = new DiscussionServer(discussantOrderCalculatorService)
    {
        Persons = persons,
        Questions = questions,
        DiscussionDuration = new TimeSpan(1, 0, 0)
    };
    discussionServer.Update();
    discussionServer.Start();
}

Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 6m - {discussionServer.NextQuestion(new TimeSpan(0, 6, 0))}");
Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 9m - {discussionServer.NextQuestion(new TimeSpan(0, 9, 0))}");
Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 5m - {discussionServer.NextQuestion(new TimeSpan(0, 2, 0))}");
Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 6m - {discussionServer.NextQuestion(new TimeSpan(0, 6, 0))}");
Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 9m - {discussionServer.NextQuestion(new TimeSpan(0, 9, 0))}");
Console.WriteLine($"{discussionServer.CurrentRound!.CurrentDiscussant!.Person.Name}: 5m - {discussionServer.NextQuestion(new TimeSpan(0, 2, 0))}");

string ToHMSFromTimeSpan(TimeSpan duration)
{
    var result = "";
    if (duration.Hours > 0) result += duration.Hours + "h";
    if (duration.Minutes > 0) result += duration.Minutes + "m";
    if (duration.Seconds > 0) result += duration.Seconds + "s";

    return (result.Length == 0 ? "0m" : result);
}

var i = 0;
foreach (var round in discussionServer.Rounds)
{
    Console.WriteLine(
        $"{i+1}. " +
        string.Join(" - ", round.Discussants.Select((d, i) => $"{d.Person.Name}={ToHMSFromTimeSpan(round.DiscussantsTimes[i]!.AllowedDurationMin)} {ToHMSFromTimeSpan(round.DiscussantsTimes[i]!.UsedDuration)} {ToHMSFromTimeSpan(round.DiscussantsTimes[i]!.AllowedDurationMax)}"))
    );
    i++;
}

var y = 0;

//object discussionA;
//{
//    var mathService = new MathService();
//    var randomService = new RandomService();
//    var shuffleService = new ShuffleService(randomService);
//    var discussantOrderCalculatorService = new DiscussantOrderDistributionCalculatorService(mathService, randomService, shuffleService);
//    foreach (var service in new IService[] { mathService, randomService, shuffleService, discussantOrderCalculatorService })
//    {
//        service.Init();
//    }

//    discussionA = new PanelDiscussion(
//        discussantOrderCalculatorService
//    )
//    {
//        Discussants = disscutants,
//        Questions = questions
//    };
//}

//IPreparedDiscussion discussionB;
//{
//    var mathService = new MathService();
//    var randomService = new RandomService();
//    var shuffleService = new ShuffleService(randomService);
//    var discussantOrderCalculatorService = new DiscussantOrderDistributionCalculatorService(mathService, randomService, shuffleService)
//    {
//        Weights = new int[] { 2, 4, 3, 2, 1}
//    };
//    foreach (var service in new IService[] { mathService, randomService, shuffleService, discussantOrderCalculatorService })
//    {
//        service.Init();
//    }

//    discussionB = new PanelDiscussion(
//        discussantOrderCalculatorService
//    )
//    {
//        Discussants = disscutants,
//        Questions = questions
//    };
//}

//IPreparedDiscussion discussionC;
//{
//    var mathService = new MathService();
//    var randomService = new RandomService();
//    var shuffleService = new ShuffleService(randomService);
//    var discussantOrderCalculatorService = new DiscussantOrderRandomCalculatorService(mathService, randomService, shuffleService);
//    foreach (var service in new IService[] { mathService, randomService, shuffleService, discussantOrderCalculatorService })
//    {
//        service.Init();
//    }

//    discussionC = new PanelDiscussion(
//        discussantOrderCalculatorService
//    )
//    {
//        Discussants = disscutants,
//        Questions = questions
//    };
//}

//discussionA.UpdateQuestionRounds();

//discussionA.Start();

//discussionB.UpdateQuestionRounds();

//discussionB.Start();

//discussionC.UpdateQuestionRounds();

//discussionC.Start();