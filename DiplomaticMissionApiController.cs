﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Sabio.Models;
using Sabio.Models.Domain.DiplomaticMissions;
using Sabio.Models.Requests.DiplomaticMissions;
using Sabio.Services;
using Sabio.Services.Interfaces;
using Sabio.Web.Controllers;
using Sabio.Web.Models.Responses;
using System;

namespace Sabio.Web.Api.Controllers
{

    [Route("api/diplomaticmissions")]
    [ApiController]
    public class DiplomaticMissionApiController : BaseApiController
    {


        private IDiplomaticMissionService _service = null;
        private IAuthenticationService<int> _authService = null;

        public DiplomaticMissionApiController(IDiplomaticMissionService service
        , ILogger<DiplomaticMissionApiController> logger
        , IAuthenticationService<int> authService) : base(logger)

        {

            _service = service;
            _authService = authService;

        }

        [HttpPost("scrape")]
        public ActionResult<ItemResponse<SuccessResponse>> ScrapeEmbassyInfo(string url)
        {

            int code = 200;
            BaseResponse response = null;

            try
            {
                _service.ScrapeEmbassyInfo("https://travel.state.gov/content/travel/en/us-visas/visa-information-resources/list-of-posts.html");
                if (response == null)
                {
                    code = 404;
                    response = new ErrorResponse("No HTML document found");
                }
                else
                {

                    response = new SuccessResponse();

                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());

            }
            return StatusCode(code, response); ;
        }


        [HttpGet("")]
        public ActionResult<ItemResponse<Paged<DiplomaticMission>>> GetAllByPage(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<DiplomaticMission> paged = _service.GetAllByPage(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Records Not Found"));
                }
                else
                {
                    ItemResponse<Paged<DiplomaticMission>> response = new ItemResponse<Paged<DiplomaticMission>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpGet("embassies")]
        public ActionResult<ItemResponse<Paged<DiplomaticMission>>> GetAllByEmbassy(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<DiplomaticMission> paged = _service.GetAllByEmbassy(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Records Not Found"));
                }
                else
                {
                    ItemResponse<Paged<DiplomaticMission>> response = new ItemResponse<Paged<DiplomaticMission>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpGet("consulates")]
        public ActionResult<ItemResponse<Paged<DiplomaticMission>>> GetAllByConsulate(int pageIndex, int pageSize)
        {
            ActionResult result = null;
            try
            {
                Paged<DiplomaticMission> paged = _service.GetAllByConsulate(pageIndex, pageSize);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Records Not Found"));
                }
                else
                {
                    ItemResponse<Paged<DiplomaticMission>> response = new ItemResponse<Paged<DiplomaticMission>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpGet("country/{countryId:int}")]
        public ActionResult<ItemResponse<Paged<DiplomaticMission>>> GetAllByCountryId(int pageIndex, int pageSize, int countryId)
        {
            ActionResult result = null;
            try
            {
                Paged<DiplomaticMission> paged = _service.GetAllByCountryId(pageIndex, pageSize, countryId);
                if (paged == null)
                {
                    result = NotFound404(new ErrorResponse("Records Not Found"));
                }
                else
                {
                    ItemResponse<Paged<DiplomaticMission>> response = new ItemResponse<Paged<DiplomaticMission>>();
                    response.Item = paged;
                    result = Ok200(response);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.ToString());
                result = StatusCode(500, new ErrorResponse(ex.Message.ToString()));
            }
            return result;
        }

        [HttpPost("")]
        public ActionResult<int> Add(DiplomaticMissionAddRequest model)
        {

            ObjectResult result = null;

            try
            {

                int id = _service.Add(model);

                ItemResponse<int> response = new ItemResponse<int>() { Item = id };

                result = Created201(response);

            }

            catch (Exception ex)

            {

                base.Logger.LogError(ex.ToString());
                ErrorResponse response = new ErrorResponse(ex.Message);

                result = StatusCode(500, response);

            }

            return result;

        }

        [HttpPut("{id:int}")]
        public ActionResult<SuccessResponse> Update(DiplomaticMissionUpdateRequest model)
        {

            int iCode = 200;

            BaseResponse response = null;

            try
            {

                _service.Update(model);

                response = new SuccessResponse();

            }
            catch (Exception ex)
            {

                iCode = 500;
                response = new ErrorResponse(ex.Message);

            }

            return StatusCode(iCode, response);
        }

    }
}