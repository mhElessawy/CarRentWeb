using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRentWeb.Migrations
{
    /// <inheritdoc />
    public partial class NewRent11 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DeffType",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeffType", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserFullName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserMobile = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserAdmin = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmpId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    RecievedPassword = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompView = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CompSave = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CompDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmpView = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmpSave = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmpDelete = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EmpArchive = table.Column<bool>(type: "bit", nullable: false),
                    CarView = table.Column<bool>(type: "bit", nullable: false),
                    CarSave = table.Column<bool>(type: "bit", nullable: false),
                    Cardelete = table.Column<bool>(type: "bit", nullable: false),
                    CarArchive = table.Column<bool>(type: "bit", nullable: false),
                    ContractDailyView = table.Column<bool>(type: "bit", nullable: false),
                    ContractDailySave = table.Column<bool>(type: "bit", nullable: false),
                    ContractDailyEnd = table.Column<bool>(type: "bit", nullable: false),
                    ContractMonthlyView = table.Column<bool>(type: "bit", nullable: false),
                    ContractChangeCar = table.Column<bool>(type: "bit", nullable: false),
                    ContractMonthlySave = table.Column<bool>(type: "bit", nullable: false),
                    ContractMonthlyEnd = table.Column<bool>(type: "bit", nullable: false),
                    ContractArchive = table.Column<bool>(type: "bit", nullable: false),
                    PayView = table.Column<bool>(type: "bit", nullable: false),
                    PaySave = table.Column<bool>(type: "bit", nullable: false),
                    PayDelete = table.Column<bool>(type: "bit", nullable: false),
                    DebitView = table.Column<bool>(type: "bit", nullable: false),
                    DebitSave = table.Column<bool>(type: "bit", nullable: false),
                    DebitUpdate = table.Column<bool>(type: "bit", nullable: false),
                    DebitDelete = table.Column<bool>(type: "bit", nullable: false),
                    DebitPay = table.Column<bool>(type: "bit", nullable: false),
                    VacationView = table.Column<bool>(type: "bit", nullable: false),
                    DebitLatePay = table.Column<bool>(type: "bit", nullable: false),
                    ContractChangeRent = table.Column<bool>(type: "bit", nullable: false),
                    RecievedMoney = table.Column<bool>(type: "bit", nullable: false),
                    PurshaseView = table.Column<bool>(type: "bit", nullable: false),
                    PurshaseSave = table.Column<bool>(type: "bit", nullable: false),
                    PurshaseDelete = table.Column<bool>(type: "bit", nullable: false),
                    PurshaseShowAll = table.Column<bool>(type: "bit", nullable: false),
                    PurshaseUpdate = table.Column<bool>(type: "bit", nullable: false),
                    EmpTakeMoney = table.Column<bool>(type: "bit", nullable: false),
                    ViolationView = table.Column<bool>(type: "bit", nullable: false),
                    ViolationSave = table.Column<bool>(type: "bit", nullable: false),
                    ViolationUpdate = table.Column<bool>(type: "bit", nullable: false),
                    ViolationDelete = table.Column<bool>(type: "bit", nullable: false),
                    CompDebitView = table.Column<bool>(type: "bit", nullable: false),
                    CompDebitSave = table.Column<bool>(type: "bit", nullable: false),
                    CompDebitDelete = table.Column<bool>(type: "bit", nullable: false),
                    CompDebitUpdate = table.Column<bool>(type: "bit", nullable: false),
                    CompanyData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordData", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Deff",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeffName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeffType = table.Column<int>(type: "int", nullable: true),
                    DeffCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DeffParent = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeffNameEng = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Deff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Deff_DeffType",
                        column: x => x.DeffType,
                        principalTable: "DeffType",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeTakeMoney",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TakeUserId = table.Column<int>(type: "int", nullable: false),
                    TakeMoney = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TakeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    DeleteFlag = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    TakeMoneyNo = table.Column<int>(type: "int", nullable: false),
                    EmpReson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeTakeMoney", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeTakeMoneyTakeUser_PasswordData",
                        column: x => x.TakeUserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeTakeMoneyUser_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompCode = table.Column<int>(type: "int", nullable: true),
                    CompNameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompNameEn = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompLogo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    OwnerName1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerCivilID1 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    OwnerName2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerCivilID2 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    OwnerName3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OwnerCivilID3 = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    AddressLocation = table.Column<int>(type: "int", nullable: true),
                    AddressArea = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressQasima = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressLevel = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressType = table.Column<int>(type: "int", nullable: true),
                    AddressTypeNo = table.Column<int>(type: "int", nullable: true),
                    AddressAutoNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AddressRent = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StartRent = table.Column<DateOnly>(type: "date", nullable: false),
                    EndRent = table.Column<DateOnly>(type: "date", nullable: false),
                    CivilID = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CompActivateId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CompFileNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CompLicenseNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartLicense = table.Column<DateOnly>(type: "date", nullable: false),
                    EndLicense = table.Column<DateOnly>(type: "date", nullable: false),
                    CenterNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CommercialRegistrationNo = table.Column<int>(type: "int", nullable: false),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    Tel1 = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    Tel2 = table.Column<string>(type: "nvarchar(12)", maxLength: 12, nullable: true),
                    CompReleaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CityId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyInfo_Deff",
                        column: x => x.LocationId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyInfo_Deff2",
                        column: x => x.CompActivateId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyInfo_Deff_CityId",
                        column: x => x.CityId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DeffInformation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DebitPayLateDay = table.Column<double>(type: "float", nullable: true),
                    DebitPayLatId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeffInformation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DeffInformation_Deff_DebitPayLatId",
                        column: x => x.DebitPayLatId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Purshase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PurshaseNo = table.Column<int>(type: "int", nullable: true),
                    PurshaseId = table.Column<int>(type: "int", nullable: true),
                    PurshasePayed = table.Column<decimal>(type: "money", nullable: true),
                    PurshaseDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PurshaseDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PurshaseBillNo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    EmpId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CompId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserId = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Purshase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Purshase_Deff",
                        column: x => x.PurshaseId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CarInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyID = table.Column<int>(type: "int", nullable: true),
                    CarCode = table.Column<int>(type: "int", nullable: true),
                    RegDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CarTypeId = table.Column<int>(type: "int", nullable: true),
                    CarKindId = table.Column<int>(type: "int", nullable: true),
                    CarShapeId = table.Column<int>(type: "int", nullable: true),
                    CarModel = table.Column<int>(type: "int", nullable: true),
                    CarNoOfSystemRound = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarShase = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarColor = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CarEndLicense = table.Column<DateOnly>(type: "date", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CarReg = table.Column<DateOnly>(type: "date", nullable: true),
                    CarEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CarPaper = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CarCredit = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    NoOfCredit = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarInfo_CompanyInfo",
                        column: x => x.CompanyID,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CarInfo_Deff",
                        column: x => x.CarTypeId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CarInfo_Deff1",
                        column: x => x.CarShapeId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CarInfo_Deff3",
                        column: x => x.CarKindId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CarInfo_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyInfoAtt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompId = table.Column<int>(type: "int", nullable: false),
                    TitleData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PathFileData = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyInfoAtt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyInfoAtt_CompanyInfo",
                        column: x => x.CompId,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpCode = table.Column<int>(type: "int", nullable: true),
                    FirstNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThirdNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ForthNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastNameAr = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FirstNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SecondNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ThirdNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ForthNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LastNameEn = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CivilID = table.Column<string>(type: "nvarchar(13)", maxLength: 13, nullable: true),
                    ResNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ResEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    JobTitleId = table.Column<int>(type: "int", nullable: true),
                    NationalityId = table.Column<int>(type: "int", nullable: true),
                    RelationId = table.Column<int>(type: "int", nullable: true),
                    EmpBirthDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PassportNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    PassportEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RegDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Salary = table.Column<decimal>(type: "money", nullable: true),
                    EmpPic = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpAddress = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: true),
                    TelNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    MobiileNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AutoAddressNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FullNameAr = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullNameEN = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    LocationId = table.Column<int>(type: "int", nullable: true),
                    StartLicense = table.Column<DateOnly>(type: "date", nullable: true),
                    EndLicense = table.Column<DateOnly>(type: "date", nullable: true),
                    StartPerm = table.Column<DateOnly>(type: "date", nullable: true),
                    EndPerm = table.Column<DateOnly>(type: "date", nullable: true),
                    CivilIDEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    EmpDepMang = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_CompanyInfo",
                        column: x => x.CompanyId,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_Deff",
                        column: x => x.JobTitleId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_Deff1",
                        column: x => x.NationalityId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_Deff2",
                        column: x => x.RelationId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_Deff3",
                        column: x => x.LocationId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EmployeeInfo_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserCompanyNotAppear",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CompanyId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCompanyNotAppear", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserCompanyNotAppear_CompanyInfo",
                        column: x => x.CompanyId,
                        principalTable: "CompanyInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_UserCompanyNotAppear_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CarInfoAtt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    TitleData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathFileData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CarInfoAtt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CarInfoAtt_CarInfo",
                        column: x => x.CarId,
                        principalTable: "CarInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyDebit",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompDebitNo = table.Column<int>(type: "int", nullable: false),
                    DebitQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    DebitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    PayedQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ReminderQty = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    DeletFlag = table.Column<int>(type: "int", nullable: false),
                    DebitReson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmpId = table.Column<int>(type: "int", nullable: false),
                    OtherData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDebit", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyDebit_Employee",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyDebit_UserInfo",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyDebit_UserInfoRecieve",
                        column: x => x.UserRecievedId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Contract",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    ContractNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ContractDate = table.Column<DateOnly>(type: "date", nullable: true),
                    StartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NoOfDays = table.Column<int>(type: "int", nullable: true),
                    DailyCredit = table.Column<decimal>(type: "money", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TotalCost = table.Column<decimal>(type: "money", nullable: true),
                    ContractEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ContractEndReson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ContractType = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    CreditStartDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreditEndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreditNoOfMonth = table.Column<int>(type: "int", nullable: true),
                    CreditMonthPay = table.Column<decimal>(type: "money", nullable: true),
                    CreditTotalCost = table.Column<decimal>(type: "money", nullable: true),
                    HaveVacation = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contract", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contract_CarInfo",
                        column: x => x.CarId,
                        principalTable: "CarInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_EmployeeInfo",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contract_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DebitInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DebitNo = table.Column<int>(type: "int", nullable: true),
                    EmpId = table.Column<int>(type: "int", nullable: true),
                    DebitTypeId = table.Column<int>(type: "int", nullable: true),
                    DebitDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DebitQty = table.Column<decimal>(type: "money", nullable: true),
                    DebitDescrp = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ViolationId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeleteReson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DebitPayed = table.Column<decimal>(type: "money", nullable: true, defaultValue: 0m),
                    DebitRemaining = table.Column<decimal>(type: "money", nullable: true, defaultValue: 0m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitInfo_Deff2",
                        column: x => x.DebitTypeId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitInfo_Deff3",
                        column: x => x.ViolationId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitInfo_EmployeeInfo",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitInfo_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeInfoAtt",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    TitleData = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PathFileData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeInfoAtt", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeInfoAtt_EmployeeInfo",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EmployeeSalary",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpId = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    EmpSalaryYear = table.Column<int>(type: "int", nullable: false),
                    EmpSalaryMonth = table.Column<int>(type: "int", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "date", nullable: false),
                    EmpSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    EmpSpecial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddBasicSalary = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddFood = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddSpecial = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddTravel = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AddRoom = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductPenality = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductAdvance = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DeductRoom = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalPayed = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalDeduuct = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Net = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Approve = table.Column<int>(type: "int", nullable: false),
                    SalaryRecieved = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeSalary", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmployeeSalary_EmployeeInfo",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeSalary_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vacation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmpId = table.Column<int>(type: "int", nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NoOfDays = table.Column<int>(type: "int", nullable: true),
                    VacationPayed = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    VacationStatus = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vacation", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vacation_EmployeeInfo",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ViolationInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CarId = table.Column<int>(type: "int", nullable: true),
                    ViolationDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ViolationTime = table.Column<DateTime>(type: "datetime", nullable: true),
                    ViolationPlace = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ViolationSpeed = table.Column<double>(type: "float", nullable: true),
                    ViolationGuideId = table.Column<int>(type: "int", nullable: true),
                    ViolationCost = table.Column<decimal>(type: "money", nullable: true),
                    ViolationPoint = table.Column<int>(type: "int", nullable: true),
                    TransfereToDebit = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ViolationBookNo = table.Column<int>(type: "int", nullable: true),
                    EmpId = table.Column<int>(type: "int", nullable: true),
                    ViolationNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ViolationInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ViolationInfo_CarInfo",
                        column: x => x.CarId,
                        principalTable: "CarInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViolationInfo_Deff",
                        column: x => x.ViolationGuideId,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViolationInfo_EmployeeInfo",
                        column: x => x.EmpId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ViolationInfo_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CompanyDebitDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompDebitId = table.Column<int>(type: "int", nullable: true),
                    CompDebitDetailsNo = table.Column<int>(type: "int", nullable: false),
                    CompDebitPayed = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    CompDebitDate = table.Column<DateOnly>(type: "date", nullable: false),
                    CompDebitType = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<int>(type: "int", nullable: true),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true),
                    UserRecievedDate = table.Column<DateOnly>(type: "date", nullable: false),
                    UserRecievedNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyDebitDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyDebitDetails_CompanyDebit",
                        column: x => x.CompDebitId,
                        principalTable: "CompanyDebit",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CompanyDebitDetails_UserInfo",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CompanyDebitDetails_UserInfoRecieve",
                        column: x => x.UserRecievedId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Bill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BillNo = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NoOfDays = table.Column<int>(type: "int", nullable: true),
                    BillDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BillTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    BillPayed = table.Column<decimal>(type: "money", nullable: true),
                    LateDays = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BankIntNo = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    BankBillNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BillHent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, defaultValue: "no"),
                    DeleteReson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: true),
                    UserRecievedNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bill_Contract",
                        column: x => x.ContractId,
                        principalTable: "Contract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bill_Deff",
                        column: x => x.BankIntNo,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bill_EmployeeInfo",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Bill_PasswordData",
                        column: x => x.userId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "CreditBill",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreditBillNo = table.Column<int>(type: "int", nullable: true),
                    EmployeeId = table.Column<int>(type: "int", nullable: true),
                    FromDate = table.Column<DateOnly>(type: "date", nullable: true),
                    ToDate = table.Column<DateOnly>(type: "date", nullable: true),
                    NoOfMonth = table.Column<int>(type: "int", nullable: true),
                    CreditBillDate = table.Column<DateOnly>(type: "date", nullable: true),
                    CreditBillTime = table.Column<TimeOnly>(type: "time", nullable: true),
                    CreditBillPayed = table.Column<decimal>(type: "money", nullable: true),
                    LateMonth = table.Column<int>(type: "int", nullable: true),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BankIntNo = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    BankBillNo = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BankDate = table.Column<DateOnly>(type: "date", nullable: true),
                    BillHent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true, defaultValue: "no"),
                    DeleteReson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    userId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CreditBill", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CreditBill_Contract",
                        column: x => x.ContractId,
                        principalTable: "Contract",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditBill_Deff",
                        column: x => x.BankIntNo,
                        principalTable: "Deff",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditBill_EmployeeInfo",
                        column: x => x.EmployeeId,
                        principalTable: "EmployeeInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_CreditBill_PasswordData",
                        column: x => x.userId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DebitPayInfo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DebitPayNo = table.Column<int>(type: "int", nullable: true),
                    DebitPayDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DebitPayQty = table.Column<decimal>(type: "money", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    ViolationId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Hent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DeleteReson = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DebitInfoId = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    UserRecievedNo = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DebitPayInfo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DebitPayInfo_DebitInfo",
                        column: x => x.DebitInfoId,
                        principalTable: "DebitInfo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitPayInfo_PasswordData",
                        column: x => x.UserId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitPayInfo_PasswordData1",
                        column: x => x.UserRecievedId,
                        principalTable: "PasswordData",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DebitPayInfo_ViolationInfo",
                        column: x => x.ViolationId,
                        principalTable: "ViolationInfo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContractDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ContractId = table.Column<int>(type: "int", nullable: true),
                    DailyCreditDate = table.Column<DateOnly>(type: "date", nullable: true),
                    DailyCredit = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    BillId = table.Column<int>(type: "int", nullable: true),
                    PayedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: true, defaultValue: 0),
                    CarCredit = table.Column<decimal>(type: "decimal(18,3)", nullable: true),
                    DeleteFlag = table.Column<int>(type: "int", nullable: true, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContractDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContractDetails_Bill",
                        column: x => x.BillId,
                        principalTable: "Bill",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ContractDetails_Contract",
                        column: x => x.ContractId,
                        principalTable: "Contract",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Bill_BankIntNo",
                table: "Bill",
                column: "BankIntNo");

            migrationBuilder.CreateIndex(
                name: "IX_Bill_ContractId",
                table: "Bill",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_Bill_EmployeeId",
                table: "Bill",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Bill_userId",
                table: "Bill",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfo_CarKindId",
                table: "CarInfo",
                column: "CarKindId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfo_CarShapeId",
                table: "CarInfo",
                column: "CarShapeId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfo_CarTypeId",
                table: "CarInfo",
                column: "CarTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfo_CompanyID",
                table: "CarInfo",
                column: "CompanyID");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfo_UserId",
                table: "CarInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CarInfoAtt_CarId",
                table: "CarInfoAtt",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebit_CompDebitNo",
                table: "CompanyDebit",
                column: "CompDebitNo");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebit_DebitDate",
                table: "CompanyDebit",
                column: "DebitDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebit_EmpId",
                table: "CompanyDebit",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebit_UserId",
                table: "CompanyDebit",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebit_UserRecievedId",
                table: "CompanyDebit",
                column: "UserRecievedId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebitDetails_CompDebitDate",
                table: "CompanyDebitDetails",
                column: "CompDebitDate");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebitDetails_CompDebitId",
                table: "CompanyDebitDetails",
                column: "CompDebitId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebitDetails_UserId",
                table: "CompanyDebitDetails",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyDebitDetails_UserRecievedId",
                table: "CompanyDebitDetails",
                column: "UserRecievedId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_CityId",
                table: "CompanyInfo",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_CompActivateId",
                table: "CompanyInfo",
                column: "CompActivateId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfo_LocationId",
                table: "CompanyInfo",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_CompanyInfoAtt_CompId",
                table: "CompanyInfoAtt",
                column: "CompId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_CarId",
                table: "Contract",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_EmployeeId",
                table: "Contract",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Contract_UserId",
                table: "Contract",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDetails_BillId",
                table: "ContractDetails",
                column: "BillId");

            migrationBuilder.CreateIndex(
                name: "IX_ContractDetails_ContractId",
                table: "ContractDetails",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBill_BankIntNo",
                table: "CreditBill",
                column: "BankIntNo");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBill_ContractId",
                table: "CreditBill",
                column: "ContractId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBill_EmployeeId",
                table: "CreditBill",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_CreditBill_userId",
                table: "CreditBill",
                column: "userId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitInfo_DebitTypeId",
                table: "DebitInfo",
                column: "DebitTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitInfo_EmpId",
                table: "DebitInfo",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitInfo_UserId",
                table: "DebitInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitInfo_ViolationId",
                table: "DebitInfo",
                column: "ViolationId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitPayInfo_DebitInfoId",
                table: "DebitPayInfo",
                column: "DebitInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitPayInfo_UserId",
                table: "DebitPayInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitPayInfo_UserRecievedId",
                table: "DebitPayInfo",
                column: "UserRecievedId");

            migrationBuilder.CreateIndex(
                name: "IX_DebitPayInfo_ViolationId",
                table: "DebitPayInfo",
                column: "ViolationId");

            migrationBuilder.CreateIndex(
                name: "IX_Deff_DeffType",
                table: "Deff",
                column: "DeffType");

            migrationBuilder.CreateIndex(
                name: "IX_DeffInformation_DebitPayLatId",
                table: "DeffInformation",
                column: "DebitPayLatId",
                unique: true,
                filter: "[DebitPayLatId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_CompanyId",
                table: "EmployeeInfo",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_JobTitleId",
                table: "EmployeeInfo",
                column: "JobTitleId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_LocationId",
                table: "EmployeeInfo",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_NationalityId",
                table: "EmployeeInfo",
                column: "NationalityId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_RelationId",
                table: "EmployeeInfo",
                column: "RelationId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfo_UserId",
                table: "EmployeeInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeInfoAtt_EmpId",
                table: "EmployeeInfoAtt",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_EmpId",
                table: "EmployeeSalary",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeSalary_UserId",
                table: "EmployeeSalary",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTakeMoney_TakeUserId",
                table: "EmployeeTakeMoney",
                column: "TakeUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeTakeMoney_UserId",
                table: "EmployeeTakeMoney",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Purshase_PurshaseId",
                table: "Purshase",
                column: "PurshaseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyNotAppear_CompanyId",
                table: "UserCompanyNotAppear",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCompanyNotAppear_UserId",
                table: "UserCompanyNotAppear",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Vacation_EmpId",
                table: "Vacation",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationInfo_CarId",
                table: "ViolationInfo",
                column: "CarId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationInfo_EmpId",
                table: "ViolationInfo",
                column: "EmpId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationInfo_UserId",
                table: "ViolationInfo",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_ViolationInfo_ViolationGuideId",
                table: "ViolationInfo",
                column: "ViolationGuideId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CarInfoAtt");

            migrationBuilder.DropTable(
                name: "CompanyDebitDetails");

            migrationBuilder.DropTable(
                name: "CompanyInfoAtt");

            migrationBuilder.DropTable(
                name: "ContractDetails");

            migrationBuilder.DropTable(
                name: "CreditBill");

            migrationBuilder.DropTable(
                name: "DebitPayInfo");

            migrationBuilder.DropTable(
                name: "DeffInformation");

            migrationBuilder.DropTable(
                name: "EmployeeInfoAtt");

            migrationBuilder.DropTable(
                name: "EmployeeSalary");

            migrationBuilder.DropTable(
                name: "EmployeeTakeMoney");

            migrationBuilder.DropTable(
                name: "Purshase");

            migrationBuilder.DropTable(
                name: "UserCompanyNotAppear");

            migrationBuilder.DropTable(
                name: "Vacation");

            migrationBuilder.DropTable(
                name: "CompanyDebit");

            migrationBuilder.DropTable(
                name: "Bill");

            migrationBuilder.DropTable(
                name: "DebitInfo");

            migrationBuilder.DropTable(
                name: "ViolationInfo");

            migrationBuilder.DropTable(
                name: "Contract");

            migrationBuilder.DropTable(
                name: "CarInfo");

            migrationBuilder.DropTable(
                name: "EmployeeInfo");

            migrationBuilder.DropTable(
                name: "CompanyInfo");

            migrationBuilder.DropTable(
                name: "PasswordData");

            migrationBuilder.DropTable(
                name: "Deff");

            migrationBuilder.DropTable(
                name: "DeffType");
        }
    }
}
