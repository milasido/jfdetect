using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using jf.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.CognitiveServices.Vision.Face;
using Microsoft.Azure.CognitiveServices.Vision.Face.Models;

namespace jf.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FaceController : ControllerBase
    {

        private IFaceClient faceClient = new FaceClient(
            new ApiKeyServiceClientCredentials("86dd1ef25ddc46238bd36c7583a0db2e"))
        { Endpoint = "https://eastus.api.cognitive.microsoft.com/" };


        //const int PersonCount = 10000;{
        const int CallLimitPerSecond = 10;
        static Queue<DateTime> _timeStampQueue = new Queue<DateTime>(CallLimitPerSecond);
        

        static async Task WaitCallLimitPerSecondAsync()
        {
            
            Monitor.Enter(_timeStampQueue);
            try
            {
                if (_timeStampQueue.Count >= CallLimitPerSecond)
                {
                    TimeSpan timeInterval = DateTime.UtcNow - _timeStampQueue.Peek();
                    if (timeInterval < TimeSpan.FromSeconds(1))
                    {
                        await Task.Delay(TimeSpan.FromSeconds(1) - timeInterval);
                    }
                    _timeStampQueue.Dequeue();
                }
                _timeStampQueue.Enqueue(DateTime.UtcNow);
            }
            finally
            {
                Monitor.Exit(_timeStampQueue);
            }
        }


        // api/Face/addfaces
        [HttpPost("addfaces")]
        public async Task<IActionResult> AddFaces(TrainObject train)
        {
            //get all folder's names to prepare to train
            DirectoryInfo direct = new DirectoryInfo(train.PathFolder);
            DirectoryInfo[] listOfFolder = direct.GetDirectories();
            int PersonCount = listOfFolder.Length;

            string personGroupId = train.GroupName;
            string personGroupName = train.GroupName;
            _timeStampQueue.Enqueue(DateTime.UtcNow);

            //if persongroupID has not already been create yet
            //string status = "not created yet, created successful";
            //if (await faceClient.LargePersonGroup.GetAsync(personGroupId) == null)
            //var allLargePersonGroups = await faceClient.LargePersonGroup.ListAsync();
            //if (allLargePersonGroups?.Any(x=>x.LargePersonGroupId == personGroupId)==false)
            {
            await faceClient.PersonGroup.CreateAsync(personGroupId, personGroupName);
            }
            //else status = "group already had";


            // create persons for the persongroup
            Person[] persons = new Person[PersonCount];
            //Parallel.For(0, PersonCount, async i =>
            for (int i=0; i<PersonCount; i++)
            {
                //await WaitCallLimitPerSecondAsync();
                string personName = $"{listOfFolder[i].Name}";
                //if(await faceClient.PersonGroupPerson. GetAsync(personName) == null)
                    persons[i] = await faceClient.PersonGroupPerson.CreateAsync(personGroupId, personName);
                //Thread.Sleep(1);
            };



            // add and train faces to the persons in personGroup
            //Parallel.For(0, PersonCount, async i =>
            for (int i = 0; i < PersonCount; i++)
            {
                //await WaitCallLimitPerSecondAsync();
                //Guid personId = persons[i].PersonId;
                string personImageDir = @$"{train.PathFolder}\{listOfFolder[i].Name}";

                foreach (string imagePath in Directory.GetFiles(personImageDir, "*.jpg"))
                {
                    //await WaitCallLimitPerSecondAsync();
                    using (Stream stream = System.IO.File.OpenRead(imagePath))
                    {
                        try
                        {
                            await faceClient.PersonGroupPerson.AddFaceFromStreamAsync(personGroupId, persons[i].PersonId, stream);
                        }
                        catch
                        {
                            continue;
                        }                  
                    }                  
                }
            };

            //await faceClient.PersonGroup.TrainAsync(personGroupId);
            //TrainingStatus trainingStatus = null;
            //while (true)
            //{
            //    trainingStatus = await faceClient.PersonGroup.GetTrainingStatusAsync(personGroupId);
            //    if (trainingStatus.Status != TrainingStatusType.Running)
            //    {
            //        break;
            //    }
            //    await Task.Delay(1000);
            //}
            return Ok(persons);
        }

        [HttpPost("identify")]
        public async Task<IActionResult> Identify(TrainObject imgPath)
        {
            List<IdentifyObject> final = new List<IdentifyObject>();
            using (Stream s = System.IO.File.OpenRead(imgPath.PathImage))
            {
                int i = 0;
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Select(face => face.FaceId.Value).ToArray();
                var results = await faceClient.Face.IdentifyAsync(faceIds, "jav");
             
                foreach (var identifyResult in results)
                {
                    if (identifyResult.Candidates.Count == 0)
                    {
                        IdentifyObject x = new IdentifyObject();
                        x.Name = "Unknown";
                        x.Height = faces[i].FaceRectangle.Height;
                        x.Left = faces[i].FaceRectangle.Left;
                        x.Top = faces[i].FaceRectangle.Top;
                        x.Width = faces[i].FaceRectangle.Width;
                        x.Confidence = 0;
                        final.Add(x);
                        i++;
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync("jav", candidateId);
                        IdentifyObject x = new IdentifyObject();
                        x.Name = person.Name;
                        x.Height = faces[i].FaceRectangle.Height;
                        x.Left = faces[i].FaceRectangle.Left;
                        x.Top = faces[i].FaceRectangle.Top;
                        x.Width = faces[i].FaceRectangle.Width;
                        x.Confidence = identifyResult.Candidates[0].Confidence;
                        final.Add(x);
                        i++;
                    }
                }
            }
            return Ok(final);
        }


        [HttpPost("identify2")]
        public async Task<IActionResult> Identify2([FromForm] ObjectToIdentify objectToIdentify)
        {
            List<IdentifyObject> final = new List<IdentifyObject>();
            using (Stream s = objectToIdentify.Image2.OpenReadStream())
            {
                int i = 0;
                var faces = await faceClient.Face.DetectWithStreamAsync(s);
                var faceIds = faces.Select(face => face.FaceId.Value).ToArray();
                var results = await faceClient.Face.IdentifyAsync(faceIds, "jav");

                foreach (var identifyResult in results)
                {
                    if (identifyResult.Candidates.Count == 0)
                    {
                        IdentifyObject x = new IdentifyObject();
                        x.Name = "Unknown";
                        x.Height = faces[i].FaceRectangle.Height;
                        x.Left = faces[i].FaceRectangle.Left;
                        x.Top = faces[i].FaceRectangle.Top;
                        x.Width = faces[i].FaceRectangle.Width;
                        x.Confidence = 0;
                        final.Add(x);
                        i++;
                    }
                    else
                    {
                        // Get top 1 among all candidates returned
                        var candidateId = identifyResult.Candidates[0].PersonId;
                        var person = await faceClient.PersonGroupPerson.GetAsync("jav", candidateId);
                        IdentifyObject x = new IdentifyObject();
                        x.Name = person.Name;
                        x.Height = faces[i].FaceRectangle.Height;
                        x.Left = faces[i].FaceRectangle.Left;
                        x.Top = faces[i].FaceRectangle.Top;
                        x.Width = faces[i].FaceRectangle.Width;
                        x.Confidence = identifyResult.Candidates[0].Confidence;
                        final.Add(x);
                        i++;
                    }
                }
            }
            return Ok(final);
        }


    }
}