namespace RabbitMq.VHostApplication.Workers;

public class WorkerManager
{
    private Task _eventCreationTask;
    private Task _eventProcessingTask;
    private Task _eventDispatchTask;


    public void StartWorkers()
    {

        if (_eventCreationTask == null || _eventCreationTask.IsCompleted)
        {
            _eventCreationTask = Task.Run(() => new EventCreationWorker().Start());
        }

        if (_eventProcessingTask == null || _eventProcessingTask.IsCompleted)
        {
            _eventProcessingTask = Task.Run(() => new EventProcessingWorker().Start());
        }

        if (_eventDispatchTask == null || _eventDispatchTask.IsCompleted)
        {
            _eventDispatchTask = Task.Run(() => new EventDispatchWorker().Start());
        }
    }

}
