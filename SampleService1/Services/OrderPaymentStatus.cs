namespace SampleService1.Services
{
    public interface IOrderPaymentStatus
    {
        public Task HandleOrderPayment { get; set; }
    }
    public class OrderPaymentStatus
    {
    }
}
