
namespace JinkeGroup.Logic.BucketUpdateInternal
{
    public enum BucketUpdateQueueType
    {
        Add,
        Remove,
    }

    public struct BucketQueueElement
    {
        public BucketUpdateQueueType BucketUpdateQueueType;
        public BucketUpdateBehaviour BucketUpdateBehaviour;

        public BucketQueueElement(BucketUpdateQueueType bucketUpdateQueueType, BucketUpdateBehaviour bucketUpdateBehaviour)
        {
            BucketUpdateQueueType = bucketUpdateQueueType;
            BucketUpdateBehaviour = bucketUpdateBehaviour;
        }
    }
}