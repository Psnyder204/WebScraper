using HtmlAgilityPack;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models;
using Sabio.Models.Domain.DiplomaticMissions;
using Sabio.Models.Requests;
using Sabio.Models.Requests.DiplomaticMissions;
using Sabio.Services.Interfaces;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Sabio.Services
{
    public class DiplomaticMissionService : IDiplomaticMissionService
    {
        private static IDataProvider _data = null;
        private static ILocationService _locationService = null;

        public DiplomaticMissionService(IDataProvider data, ILocationService locationService)
        {

            _data = data;
            _locationService = locationService;

        }

        public void ScrapeEmbassyInfo(string url)
        {

            HtmlWeb web = new HtmlWeb();
            HtmlDocument doc = web.Load(url);

            var embassies = doc.DocumentNode.SelectNodes("//a[starts-with(@href, '/content/travel/en/us-visas/Supplements/Supplements_by_Post/')]").ToList();
            var links = embassies.Select(a => a.Attributes["href"].Value).ToList();

            List<EmbassyUrlAddRequest> embassyNodes = new();

            foreach (var item in links)
            {
                string embassyLink = null;

                embassyLink = "https://travel.state.gov" + item.ToString();
                HtmlDocument target = web.Load(embassyLink);
                var embassyinfo = target.DocumentNode.SelectNodes("//div[starts-with(@class, 'contact')]").ToList();

                EmbassyUrlAddRequest info = new();
                var embassyNode = embassyinfo[0];

                info.Name = embassyNode.SelectSingleNode("//div[@class='contact-title']").InnerText;
                info.Phone = embassyNode.SelectSingleNode("//div[@class='contact-data']").InnerText;
                info.Email = embassyNode.SelectSingleNode("//a[starts-with(@href, 'mailto')]").InnerText;
                info.Website = embassyLink;
                embassyNodes.Add(info);

                AddEmbassy(info);
            }
        }

        public int AddEmbassy(EmbassyUrlAddRequest model)
        {
            int id = 0;

            string procName = "[dbo].[EmbassyContact_Insert]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonEmbassyParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {

                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }

        public Paged<DiplomaticMission> GetAllByPage(int pageIndex, int pageSize)
        {
            Paged<DiplomaticMission> pagedList = null;
            List<DiplomaticMission> list = null;
            int totalCount = 0;

            _data.ExecuteCmd("[dbo].[DiplomaticMissions_SelectAll]",
                (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);

                },
                (IDataReader reader, short set) =>
                {

                    int index = 0;
                    DiplomaticMission diplomaticMission = MapDiplomaticMission(reader, ref index);


                    if (totalCount == 0) 
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }
                    if (list == null)
                    {
                        list = new List<DiplomaticMission>();
                    }

                    list.Add(diplomaticMission);
                }
                );

            if (list != null)
            {
                pagedList = new Paged<DiplomaticMission>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<DiplomaticMission> GetAllByEmbassy(int pageIndex, int pageSize)
        {
            Paged<DiplomaticMission> pagedList = null;
            List<DiplomaticMission> list = null;
            int totalCount = 0;

            _data.ExecuteCmd("[dbo].[DiplomacticMissions_Select_Embassies]",
                (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);

                },
                (IDataReader reader, short set) =>
                {

                    int index = 0;
                    DiplomaticMission diplomaticMission = MapDiplomaticMission(reader, ref index);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }
                    if (list == null)
                    {
                        list = new List<DiplomaticMission>();
                    }

                    list.Add(diplomaticMission);
                }
                );

            if (list != null)
            {
                pagedList = new Paged<DiplomaticMission>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<DiplomaticMission> GetAllByConsulate(int pageIndex, int pageSize)
        {
            Paged<DiplomaticMission> pagedList = null;
            List<DiplomaticMission> list = null;
            int totalCount = 0;

            _data.ExecuteCmd("[dbo].[DiplomacticMissions_Select_Consulates]",
                (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);

                },
                (IDataReader reader, short set) =>
                {

                    int index = 0;
                    DiplomaticMission diplomaticMission = MapDiplomaticMission(reader, ref index);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }
                    if (list == null)
                    {
                        list = new List<DiplomaticMission>();
                    }

                    list.Add(diplomaticMission);
                }
                );

            if (list != null)
            {
                pagedList = new Paged<DiplomaticMission>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public Paged<DiplomaticMission> GetAllByCountryId(int pageIndex, int pageSize, int countryId)
        {
            Paged<DiplomaticMission> pagedList = null;
            List<DiplomaticMission> list = null;
            int totalCount = 0;

            _data.ExecuteCmd("[dbo].[DiplomacticMissions_Select_ByCountryId]",
                (param) =>
                {
                    param.AddWithValue("@PageIndex", pageIndex);
                    param.AddWithValue("@PageSize", pageSize);
                    param.AddWithValue("@CountryId", countryId);
                },
                (IDataReader reader, short set) =>
                {

                    int index = 0;
                    DiplomaticMission diplomaticMission = MapDiplomaticMission(reader, ref index);

                    if (totalCount == 0)
                    {
                        totalCount = reader.GetSafeInt32(index++);
                    }
                    if (list == null)
                    {
                        list = new List<DiplomaticMission>();
                    }

                    list.Add(diplomaticMission);
                }
                );

            if (list != null)
            {
                pagedList = new Paged<DiplomaticMission>(list, pageIndex, pageSize, totalCount);
            }
            return pagedList;
        }

        public int Add(DiplomaticMissionAddRequest model)
        {

            int id = 0;

            string procName = "[dbo].[DiplomaticMissions_Insert]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                AddCommonParams(model, col);

                SqlParameter idOut = new SqlParameter("@Id", SqlDbType.Int);
                idOut.Direction = ParameterDirection.Output;

                col.Add(idOut);

            }, returnParameters: delegate (SqlParameterCollection returnCollection)
            {

                object oId = returnCollection["@Id"].Value;

                int.TryParse(oId.ToString(), out id);

            });

            return id;
        }

        public void Update(DiplomaticMissionUpdateRequest model)
        {
            string procName = "[dbo].[DiplomaticMissions_Update]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {

                AddCommonParams(model, col);
                col.AddWithValue("@Id", model.Id);
                col.AddWithValue("@isActive", model.IsActive);
            },

            returnParameters: null);

        }

        private static void AddCommonEmbassyParams(EmbassyUrlAddRequest model, SqlParameterCollection col)
        {


            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@Phone", model.Phone);
            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@Website", model.Website);

        }

        private static void AddCommonParams(DiplomaticMissionAddRequest model, SqlParameterCollection col)
        {

            col.AddWithValue("@CountryId", model.CountryId);
            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@LocationId", model.LocationId);
            col.AddWithValue("@Phone", model.Phone);
            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@Website", model.Website);
            col.AddWithValue("@IsEmbassy", model.IsEmbassy);
            col.AddWithValue("@IsConsulate", model.IsConsulate);

        }

        private static DiplomaticMission MapDiplomaticMission(IDataReader reader, ref int index)
        {
            DiplomaticMission diplomaticMission = new DiplomaticMission();

            diplomaticMission = new DiplomaticMission();

            diplomaticMission.Country = new Models.Domain.Countries.Country();
            diplomaticMission.Country.Id = reader.GetSafeInt32(index++);
            diplomaticMission.Country.Code = reader.GetSafeString(index++);
            diplomaticMission.Country.Name = reader.GetSafeString(index++);
            diplomaticMission.Country.Flag = reader.GetSafeString(index++);
            diplomaticMission.Id = reader.GetSafeInt32(index++);
            diplomaticMission.Name = reader.GetSafeString(index++);
            diplomaticMission.Location = _locationService.MapSingleLocation(reader,ref index);
            diplomaticMission.Phone = reader.GetSafeString(index++);
            diplomaticMission.Email = reader.GetSafeString(index++);
            diplomaticMission.Website = reader.GetSafeString(index++);
            diplomaticMission.IsActive = reader.GetSafeBool(index++);
            diplomaticMission.IsEmbassy = reader.GetSafeBool(index++);
            diplomaticMission.IsConsulate = reader.GetSafeBool(index++);

            return diplomaticMission;

        }


    }
}
