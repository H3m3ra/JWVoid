using PanelDiscussionManager;
using PanelDiscussionManager.Interfaces;
using PanelDiscussionManager.Services;

IDiscussion discussion;
{
    var randomService = new RandomService();
    var discussantOrderCalculatorService = new DiscussantOrderCalculatorService(randomService);
    foreach(var service in new IService[] { randomService, discussantOrderCalculatorService })
    {
        service.Init();
    }

    discussion = new PanelDiscussion(
        discussantOrderCalculatorService
    )
    {
        Discussants = new Discussant[] {
            new Discussant(){ Name = "L" },
            new Discussant(){ Name = "G" },
            new Discussant(){ Name = "S" },
            new Discussant(){ Name = "C" },
            new Discussant(){ Name = "F" },
            new Discussant(){ Name = "A" }
        },
        Questions = new Question[] {
            new Question("1.1"),
            //new Question("1.2"),
            //new Question("2.1"),
            new Question("2.2"),
            //new Question("3.1"),
            //new Question("3.2"),
            //new Question("4.1"),
            //new Question("4.2"),
            //new Question("5.1"),
            new Question("5.2")
        }
    };
}

discussion.UpdateQuestionRounds();

discussion.Start();