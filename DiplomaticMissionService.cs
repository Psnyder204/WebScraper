using HtmlAgilityPack;


namespace Services
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

            List<DiplomaticMissionAddRequest> embassyNodes = new();

            foreach (var item in links)
            {
                string embassyLink = null;

                embassyLink = "https://travel.state.gov" + item.ToString();
                HtmlDocument target = web.Load(embassyLink);
                var embassyinfo = target.DocumentNode.SelectNodes("//div[starts-with(@class, 'contact')]");

                if (embassyinfo == null)
                {
                    continue;
                }
                var embassyList = embassyinfo.ToList();
                
                DiplomaticMissionAddRequest info = new();
                var embassyNode = embassyinfo[0];
               
                info.CountryId = 0;
                info.Name = target.DocumentNode.SelectSingleNode("(//h1)[3]").InnerHtml; 
                info.LocationId = 0;
                info.Phone = embassyNode.SelectSingleNode("//div[@class='contact-data']").InnerText;
                info.Email = embassyNode.SelectSingleNode("//a[starts-with(@href, 'mailto')]").InnerText;
                info.Website = embassyLink;
                info.IsActive = true;
                info.IsEmbassy = info.Name.Contains("Embassy");
                info.IsConsulate = info.Name.Contains("Consulate");
                embassyNodes.Add(info);             
            }
            BatchInsert(embassyNodes);
        }

        public List<DiplomaticMissionsIdPair> BatchInsert(List<DiplomaticMissionAddRequest> model)
        {
            List<DiplomaticMissionsIdPair> ids = null;

            DataTable myParamValue = null;

            if(model != null)
            {
                 myParamValue = MapContactInfoToDiplomaticMissions(model);
            }
            _data.ExecuteCmd("[dbo].[DiplomaticMissions_BatchInsertV2]",
                inputParamMapper: delegate (SqlParameterCollection sqlParams)
                {
                    sqlParams.AddWithValue("@BatchDiplomaticMissions", myParamValue);
                },
                
                singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    DiplomaticMissionsIdPair pair = new DiplomaticMissionsIdPair();
                    int startingIndex = 0;
                    pair.NewId = reader.GetInt32(startingIndex++);
                    pair.ExternalId = reader.GetInt32(startingIndex++);

                    if (ids == null)
                    {
                        ids = new List<DiplomaticMissionsIdPair>();
                    }
                    ids.Add(pair);
                });
            return ids;      
        }

        public int AddEmbassy(DiplomaticMissionAddRequest model)
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

        public List<DiplomaticMission> GetAll()
        {
            List<DiplomaticMission> list = null;

                _data.ExecuteCmd("[dbo].[DiplomaticMissions_SelectAll_NotPaged", inputParamMapper: null
                , singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    int startingIndex = 0;

                    DiplomaticMission dm = MapDiplomaticMission(reader, ref startingIndex);



                    if (list == null)
                    {
                        list = new List<DiplomaticMission>();
                    }

                    list.Add(dm);
                });
            return list;
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

            },

            returnParameters: null);

        }

        public void Delete(DiplomaticMissionUpdateRequest model)
        {
            string procName = "[dbo].[DiplomaticMissions_Delete]";
            _data.ExecuteNonQuery(procName, inputParamMapper: delegate (SqlParameterCollection col)
            {
                col.AddWithValue("@Id", model.Id);
            },
            returnParameters: null);
        }

        private static void AddCommonParams(DiplomaticMissionAddRequest model, SqlParameterCollection col)
        {

            col.AddWithValue("@CountryId", model.CountryId);
            col.AddWithValue("@Name", model.Name);
            col.AddWithValue("@LocationId", model.LocationId);
            col.AddWithValue("@Phone", model.Phone);
            col.AddWithValue("@Email", model.Email);
            col.AddWithValue("@Website", model.Website);
            col.AddWithValue("@IsActive", model.IsActive);
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

        private DataTable MapContactInfoToDiplomaticMissions(List<DiplomaticMissionAddRequest> contacts)
        {
            DataTable dt = new DataTable();

            dt.Columns.Add("CountryId", typeof(Int32));
            dt.Columns.Add("Name", typeof(string));
            dt.Columns.Add("LocationId", typeof(Int32));
            dt.Columns.Add("Phone", typeof(string));
            dt.Columns.Add("Email", typeof(string));
            dt.Columns.Add("Website", typeof(string));
            dt.Columns.Add("IsActive", typeof(bool));
            dt.Columns.Add("IsEmbassy", typeof(bool));
            dt.Columns.Add("IsConsulate", typeof(bool));

            if(contacts != null)
            {
                foreach (DiplomaticMissionAddRequest singleContact in contacts)
                {
                    DataRow dr = dt.NewRow();
                    int startingIndex = 0;

                    dr.SetField(startingIndex++, singleContact.CountryId);
                    dr.SetField(startingIndex++, singleContact.Name);
                    dr.SetField(startingIndex++, singleContact.LocationId);
                    dr.SetField(startingIndex++, singleContact.Phone);
                    dr.SetField(startingIndex++, singleContact.Email);
                    dr.SetField(startingIndex++, singleContact.Website);
                    dr.SetField(startingIndex++, singleContact.IsActive);
                    dr.SetField(startingIndex++, singleContact.IsEmbassy);
                    dr.SetField(startingIndex++, singleContact.IsConsulate);

                    dt.Rows.Add(dr);
                }
            }            
            return dt;
        }


    }
}
