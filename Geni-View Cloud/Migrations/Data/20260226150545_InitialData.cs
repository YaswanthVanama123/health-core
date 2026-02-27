using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GeniView.Cloud.Migrations.Data
{
    /// <inheritdoc />
    public partial class InitialData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Agents",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AgentID = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperatingSystem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgentVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AgentAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Agents", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Logged = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Level = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ServerName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Https = table.Column<bool>(type: "bit", nullable: false),
                    ServerAddress = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Logger = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Callsite = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Exception = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InnerException = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExceptionData = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationUpdates",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AppId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HasUpdate = table.Column<bool>(type: "bit", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LatestVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseNotes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DownloadAddress = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationUpdates", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "Communities",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommunityID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Address_City = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Country = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Fax = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Phone = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_PostalCode = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Region = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_StreetLineOne = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_StreetLineTwo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Address_Website = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Communities", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "DeviceEventActionNotifications",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SendNotifications = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEventActionNotifications", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "MailServer",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Host = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Port = table.Column<int>(type: "int", nullable: false),
                    User = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ReplyTo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeliveryMethod = table.Column<int>(type: "int", nullable: false),
                    EnableSsl = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MailServer", x => x.ID);
                });

            migrationBuilder.CreateTable(
                name: "UserActivityHistory",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UserFullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AffectedObject = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserActivityHistory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Groups",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    GroupID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: false),
                    CreateDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CommunityID = table.Column<long>(type: "bigint", nullable: false),
                    ParentGroupID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Groups", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Groups_Communities_CommunityID",
                        column: x => x.CommunityID,
                        principalTable: "Communities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Groups_Groups_ParentGroupID",
                        column: x => x.ParentGroupID,
                        principalTable: "Groups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Batteries",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    DesignCapacity = table.Column<double>(type: "float", nullable: false),
                    DesignVoltage = table.Column<double>(type: "float", nullable: false),
                    SerialNumberCode = table.Column<long>(type: "bigint", nullable: true),
                    SerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FirmwareVersion = table.Column<int>(type: "int", nullable: false),
                    BatteryConfiguration = table.Column<int>(type: "int", nullable: false),
                    BatteryPackFirmwareVersion = table.Column<int>(type: "int", nullable: false),
                    BatteryChemistry = table.Column<int>(type: "int", nullable: false),
                    BatteryTechnology = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CommunityID = table.Column<long>(type: "bigint", nullable: false),
                    GroupID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Batteries", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Batteries_Communities_CommunityID",
                        column: x => x.CommunityID,
                        principalTable: "Communities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Batteries_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IsDeactivated = table.Column<bool>(type: "bit", nullable: false),
                    SerialNumber = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    FirmwareVersion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Manufacturer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ProductName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceType = table.Column<int>(type: "int", nullable: false),
                    CommunityID = table.Column<long>(type: "bigint", nullable: false),
                    GroupID = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.ID);
                    table.ForeignKey(
                        name: "FK_Devices_Communities_CommunityID",
                        column: x => x.CommunityID,
                        principalTable: "Communities",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Devices_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "AgentBatteryLog",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bay = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InternalBatteryLogCount = table.Column<long>(type: "bigint", nullable: true),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Agent_ID = table.Column<long>(type: "bigint", nullable: true),
                    Battery_ID = table.Column<long>(type: "bigint", nullable: true),
                    OperatingData_AverageCurrent = table.Column<double>(type: "float", nullable: false),
                    OperatingData_Current = table.Column<double>(type: "float", nullable: false),
                    OperatingData_CycleCount = table.Column<int>(type: "int", nullable: false),
                    OperatingData_Voltage = table.Column<double>(type: "float", nullable: false),
                    OperatingData_BatteryOperatingStatus_StatusA = table.Column<int>(type: "int", nullable: false),
                    OperatingData_BatteryOperatingStatus_StatusARaw = table.Column<int>(type: "int", nullable: true),
                    OperatingData_BatteryOperatingStatus_StatusAText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OperatingData_BatteryOperatingStatus_StatusB = table.Column<int>(type: "int", nullable: false),
                    OperatingData_BatteryOperatingStatus_StatusBRaw = table.Column<int>(type: "int", nullable: true),
                    OperatingData_BatteryOperatingStatus_StatusBText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SlowChangingDataA_AbsoluteStateOfCharge = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataA_CalculatedCapacity = table.Column<double>(type: "float", nullable: false),
                    SlowChangingDataA_DischargeCapacity = table.Column<double>(type: "float", nullable: false),
                    SlowChangingDataA_EndOfLifeCapacity = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataA_RelativeStateOfCharge = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataA_RemainingCapacity = table.Column<double>(type: "float", nullable: false),
                    SlowChangingDataB_AvailableEnergy = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_AvailablePower = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_AverageTimeToEmpty = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_AverageTimeToFull = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_BatteryInternalTemperature = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_DisplayBoardTemperature = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_LifeTime = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_PassedCharge = table.Column<double>(type: "float", nullable: false),
                    SlowChangingDataB_DisplayBoardStatus_ActiveLEDs = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_DisplayBoardStatus_ChargeMode = table.Column<int>(type: "int", nullable: true),
                    SlowChangingDataB_DisplayBoardStatus_IsDisplayFlashing = table.Column<bool>(type: "bit", nullable: true),
                    SlowChangingDataB_DisplayBoardStatus_IsDisplayRed = table.Column<bool>(type: "bit", nullable: true),
                    SlowChangingDataB_DisplayBoardStatus_PerceivedStateOfCharge = table.Column<int>(type: "int", nullable: false),
                    SlowChangingDataB_DisplayBoardStatus_StatusRaw = table.Column<int>(type: "int", nullable: false),
                    TimeEstimate_Estimate = table.Column<double>(type: "float", nullable: false),
                    TimeEstimate_EstimateDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeEstimate_EstimateMode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentBatteryLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AgentBatteryLog_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AgentBatteryLog_Batteries_Battery_ID",
                        column: x => x.Battery_ID,
                        principalTable: "Batteries",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "BatterySettings",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bay = table.Column<int>(type: "int", nullable: false),
                    DateOfManufacture = table.Column<DateTime>(type: "datetime2", nullable: true),
                    OemIdentifier = table.Column<int>(type: "int", nullable: false),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatteryID = table.Column<long>(type: "bigint", nullable: false),
                    AgentID = table.Column<long>(type: "bigint", nullable: false),
                    ServiceSettings_ServiceCycleCount = table.Column<int>(type: "int", nullable: false),
                    ServiceSettings_ServiceInterval = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BatterySettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_BatterySettings_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BatterySettings_Batteries_BatteryID",
                        column: x => x.BatteryID,
                        principalTable: "Batteries",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalBatteryLog",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bay = table.Column<int>(type: "int", nullable: false),
                    UniqueLogIndex = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LogIndex = table.Column<long>(type: "bigint", nullable: false),
                    EventCodeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventCode = table.Column<int>(type: "int", nullable: false),
                    EventCodeRaw = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BatterySerialNumberCode = table.Column<long>(type: "bigint", nullable: false),
                    BatterySerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteryFirmwareVersion = table.Column<int>(type: "int", nullable: false),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceLogIndex = table.Column<long>(type: "bigint", nullable: false),
                    Voltage = table.Column<double>(type: "float", nullable: false),
                    Current = table.Column<double>(type: "float", nullable: false),
                    AverageCurrent = table.Column<double>(type: "float", nullable: false),
                    RelativeStateOfCharge = table.Column<int>(type: "int", nullable: false),
                    RemainingCapacity = table.Column<double>(type: "float", nullable: false),
                    OemIdentifier = table.Column<int>(type: "int", nullable: false),
                    CycleCount = table.Column<int>(type: "int", nullable: false),
                    Temperature = table.Column<int>(type: "int", nullable: false),
                    CalculatedCapacity = table.Column<double>(type: "float", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StatusText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agent_ID = table.Column<long>(type: "bigint", nullable: true),
                    Battery_ID = table.Column<long>(type: "bigint", nullable: true),
                    BatteryOperatingStatus_StatusA = table.Column<int>(type: "int", nullable: false),
                    BatteryOperatingStatus_StatusARaw = table.Column<int>(type: "int", nullable: true),
                    BatteryOperatingStatus_StatusAText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteryOperatingStatus_StatusB = table.Column<int>(type: "int", nullable: false),
                    BatteryOperatingStatus_StatusBRaw = table.Column<int>(type: "int", nullable: true),
                    BatteryOperatingStatus_StatusBText = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalBatteryLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InternalBatteryLog_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_InternalBatteryLog_Batteries_Battery_ID",
                        column: x => x.Battery_ID,
                        principalTable: "Batteries",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "AgentDeviceLog",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceCapacity = table.Column<int>(type: "int", nullable: false),
                    IsExternalPowerInputApplied = table.Column<bool>(type: "bit", nullable: true),
                    IsExternalPowerInputNotGood = table.Column<bool>(type: "bit", nullable: true),
                    BatteriesPresentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesPresent = table.Column<int>(type: "int", nullable: false),
                    BatteriesPresentCount = table.Column<int>(type: "int", nullable: false),
                    BatteriesPoweringSystemText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesPoweringSystem = table.Column<int>(type: "int", nullable: false),
                    BatteriesPoweringSystemCount = table.Column<int>(type: "int", nullable: false),
                    BatteriesChargingText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesCharging = table.Column<int>(type: "int", nullable: false),
                    BatteriesChargingCount = table.Column<int>(type: "int", nullable: false),
                    InternalDeviceLogCount = table.Column<long>(type: "bigint", nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Agent_ID = table.Column<long>(type: "bigint", nullable: true),
                    Device_ID = table.Column<long>(type: "bigint", nullable: true),
                    Location_IsUnknown = table.Column<bool>(type: "bit", nullable: false),
                    Location_Lattitude = table.Column<double>(type: "float", nullable: false),
                    Location_Longitude = table.Column<double>(type: "float", nullable: false),
                    PowerInput_Current = table.Column<double>(type: "float", nullable: false),
                    PowerInput_Voltage = table.Column<double>(type: "float", nullable: false),
                    PowerOutput_Current = table.Column<double>(type: "float", nullable: false),
                    PowerOutput_Voltage = table.Column<double>(type: "float", nullable: false),
                    Status_Status = table.Column<int>(type: "int", nullable: false),
                    Status_StatusRaw = table.Column<int>(type: "int", nullable: true),
                    Status_StatusText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status_Temperature = table.Column<int>(type: "int", nullable: false),
                    TimeEstimate_Estimate = table.Column<double>(type: "float", nullable: false),
                    TimeEstimate_EstimateDescription = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TimeEstimate_EstimateMode = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgentDeviceLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_AgentDeviceLog_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_AgentDeviceLog_Devices_Device_ID",
                        column: x => x.Device_ID,
                        principalTable: "Devices",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DeviceEvents",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: false),
                    EventTypeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Source = table.Column<int>(type: "int", nullable: false),
                    SourceText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsHandled = table.Column<bool>(type: "bit", nullable: false),
                    Agent_ID = table.Column<long>(type: "bigint", nullable: true),
                    Device_ID = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceEvents", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_DeviceEvents_Devices_Device_ID",
                        column: x => x.Device_ID,
                        principalTable: "Devices",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateTable(
                name: "DeviceSettings",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Bays = table.Column<int>(type: "int", nullable: false),
                    OemIdentifier = table.Column<int>(type: "int", nullable: false),
                    ChargingMode = table.Column<int>(type: "int", nullable: true),
                    DischargingMode = table.Column<int>(type: "int", nullable: true),
                    BargraphDimming = table.Column<bool>(type: "bit", nullable: true),
                    DeviceTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LoggingInterval = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceID = table.Column<long>(type: "bigint", nullable: false),
                    AgentID = table.Column<long>(type: "bigint", nullable: false),
                    AlertSettings_AlertType = table.Column<int>(type: "int", nullable: false),
                    AlertSettings_DisplayMode = table.Column<int>(type: "int", nullable: false),
                    AlertSettings_LowBatteryAlertInterval = table.Column<int>(type: "int", nullable: false),
                    AlertSettings_LowBatteryAlertLevel = table.Column<int>(type: "int", nullable: false),
                    AlertSettings_SystemMode = table.Column<int>(type: "int", nullable: false),
                    BatteryStateOfChargeSettings_PercentageCutOff = table.Column<int>(type: "int", nullable: false),
                    BatteryStateOfChargeSettings_VoltageCutOff = table.Column<double>(type: "float", nullable: false),
                    PowerOutputSettings_IsDCConverterFitted = table.Column<bool>(type: "bit", nullable: true),
                    PowerOutputSettings_PotValue = table.Column<int>(type: "int", nullable: false),
                    PowerOutputSettings_Power = table.Column<long>(type: "bigint", nullable: false),
                    PowerOutputSettings_Trim = table.Column<short>(type: "smallint", nullable: false),
                    PowerOutputSettings_Voltage = table.Column<double>(type: "float", nullable: false),
                    StandbySettings_Current = table.Column<int>(type: "int", nullable: false),
                    StandbySettings_StandbyOnExternalPowerInput = table.Column<bool>(type: "bit", nullable: true),
                    StandbySettings_Time = table.Column<int>(type: "int", nullable: false),
                    SystemInformation_SlaveSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformation_SystemPartNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SystemInformation_SystemSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserInformation_Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DeviceSettings", x => x.ID);
                    table.ForeignKey(
                        name: "FK_DeviceSettings_Agents_AgentID",
                        column: x => x.AgentID,
                        principalTable: "Agents",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DeviceSettings_Devices_DeviceID",
                        column: x => x.DeviceID,
                        principalTable: "Devices",
                        principalColumn: "ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InternalDeviceLog",
                columns: table => new
                {
                    ID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LogIndex = table.Column<long>(type: "bigint", nullable: false),
                    EventCodeText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventCode = table.Column<int>(type: "int", nullable: false),
                    EventCodeRaw = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeviceSerialNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesPresentText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesPresent = table.Column<int>(type: "int", nullable: false),
                    BatteriesPresentCount = table.Column<int>(type: "int", nullable: false),
                    BatteriesPresentRaw = table.Column<int>(type: "int", nullable: false),
                    BatteriesPoweringSystemText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesPoweringSystem = table.Column<int>(type: "int", nullable: false),
                    BatteriesPoweringSystemCount = table.Column<int>(type: "int", nullable: false),
                    BatteriesPoweringSystemRaw = table.Column<int>(type: "int", nullable: false),
                    BatteriesChargingText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    BatteriesCharging = table.Column<int>(type: "int", nullable: false),
                    BatteriesChargingCount = table.Column<int>(type: "int", nullable: false),
                    BatteriesChargingRaw = table.Column<int>(type: "int", nullable: false),
                    RawData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Agent_ID = table.Column<long>(type: "bigint", nullable: true),
                    Device_ID = table.Column<long>(type: "bigint", nullable: true),
                    PowerOutput_Current = table.Column<double>(type: "float", nullable: false),
                    PowerOutput_Voltage = table.Column<double>(type: "float", nullable: false),
                    Status_Status = table.Column<int>(type: "int", nullable: false),
                    Status_StatusRaw = table.Column<int>(type: "int", nullable: true),
                    Status_StatusText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status_Temperature = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InternalDeviceLog", x => x.ID);
                    table.ForeignKey(
                        name: "FK_InternalDeviceLog_Agents_Agent_ID",
                        column: x => x.Agent_ID,
                        principalTable: "Agents",
                        principalColumn: "ID");
                    table.ForeignKey(
                        name: "FK_InternalDeviceLog_Devices_Device_ID",
                        column: x => x.Device_ID,
                        principalTable: "Devices",
                        principalColumn: "ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgentBatteryLog_Agent_ID",
                table: "AgentBatteryLog",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryID_Timestamp",
                table: "AgentBatteryLog",
                columns: new[] { "Battery_ID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_AgentDeviceLog_Agent_ID",
                table: "AgentDeviceLog",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Device_ID_Timestamp",
                table: "AgentDeviceLog",
                columns: new[] { "Device_ID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Agents_AgentID",
                table: "Agents",
                column: "AgentID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationUpdates_AppId",
                table: "ApplicationUpdates",
                column: "AppId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_CommunityID",
                table: "Batteries",
                column: "CommunityID");

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_GroupID",
                table: "Batteries",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Batteries_SerialNumberCode",
                table: "Batteries",
                column: "SerialNumberCode",
                unique: true,
                filter: "[SerialNumberCode] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySettings_AgentID",
                table: "BatterySettings",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_BatterySettings_BatteryID",
                table: "BatterySettings",
                column: "BatteryID");

            migrationBuilder.CreateIndex(
                name: "IX_Communities_CommunityID",
                table: "Communities",
                column: "CommunityID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_Agent_ID",
                table: "DeviceEvents",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceEvents_Device_ID",
                table: "DeviceEvents",
                column: "Device_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_CommunityID",
                table: "Devices",
                column: "CommunityID");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_GroupID",
                table: "Devices",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_SerialNumber",
                table: "Devices",
                column: "SerialNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSettings_AgentID",
                table: "DeviceSettings",
                column: "AgentID");

            migrationBuilder.CreateIndex(
                name: "IX_DeviceSettings_DeviceID",
                table: "DeviceSettings",
                column: "DeviceID");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CommunityID",
                table: "Groups",
                column: "CommunityID");

            migrationBuilder.CreateIndex(
                name: "IX_Groups_GroupID",
                table: "Groups",
                column: "GroupID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ParentGroupID",
                table: "Groups",
                column: "ParentGroupID");

            migrationBuilder.CreateIndex(
                name: "IX_BatteryID_Timestamp",
                table: "InternalBatteryLog",
                columns: new[] { "Battery_ID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_InternalBatteryLog_Agent_ID",
                table: "InternalBatteryLog",
                column: "Agent_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Device_ID_Timestamp",
                table: "InternalDeviceLog",
                columns: new[] { "Device_ID", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_InternalDeviceLog_Agent_ID",
                table: "InternalDeviceLog",
                column: "Agent_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgentBatteryLog");

            migrationBuilder.DropTable(
                name: "AgentDeviceLog");

            migrationBuilder.DropTable(
                name: "ApplicationLogs");

            migrationBuilder.DropTable(
                name: "ApplicationUpdates");

            migrationBuilder.DropTable(
                name: "BatterySettings");

            migrationBuilder.DropTable(
                name: "DeviceEventActionNotifications");

            migrationBuilder.DropTable(
                name: "DeviceEvents");

            migrationBuilder.DropTable(
                name: "DeviceSettings");

            migrationBuilder.DropTable(
                name: "InternalBatteryLog");

            migrationBuilder.DropTable(
                name: "InternalDeviceLog");

            migrationBuilder.DropTable(
                name: "MailServer");

            migrationBuilder.DropTable(
                name: "UserActivityHistory");

            migrationBuilder.DropTable(
                name: "Batteries");

            migrationBuilder.DropTable(
                name: "Agents");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Groups");

            migrationBuilder.DropTable(
                name: "Communities");
        }
    }
}
