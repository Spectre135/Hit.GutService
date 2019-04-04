#region using
using Hit.Auth.Filters;
using Hit.GutService.Video;
using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
#endregion

namespace Hit.GutService.Controllers
{
    [JwtAuthentication("")]
    [DeflateCompression]
    public class VideoController : ApiController
    {
        [HttpGet]
        [Route("api/play/")]
        public HttpResponseMessage Play()
        {
            HttpResponseMessage response;

            try
            {
                Service.Play();
                response = Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ApplicationException ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

            return response;
        }

        [HttpGet]
        [Route("api/stop/")]
        public HttpResponseMessage Stop()
        {
            HttpResponseMessage response;

            try
            {
                Service.Stop();
                response = Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ApplicationException ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

            return response;
        }

        [HttpGet]
        [Route("api/picture/")]
        public HttpResponseMessage Picture()
        {
            HttpResponseMessage response;

            try
            {
                Service.Picture();
                response = Request.CreateResponse(HttpStatusCode.OK);
            }
            catch (ApplicationException ex)
            {
                response = Request.CreateResponse(HttpStatusCode.InternalServerError, ex);
            }

            return response;
        }
    }
}