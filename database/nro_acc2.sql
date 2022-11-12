/*
 Navicat Premium Data Transfer

 Source Server         : local
 Source Server Type    : MySQL
 Source Server Version : 50717
 Source Host           : localhost:3306
 Source Schema         : nro_acc2

 Target Server Type    : MySQL
 Target Server Version : 50717
 File Encoding         : 65001

 Date: 04/10/2022 22:35:09
*/

SET NAMES utf8mb4;
SET FOREIGN_KEY_CHECKS = 0;

-- ----------------------------
-- Table structure for character
-- ----------------------------
DROP TABLE IF EXISTS `character`;
CREATE TABLE `character`  (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `Skills` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemBody` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemBag` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `ItemBox` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `InfoChar` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `BoughtSkill` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL,
  `PlusBag` int(11) NULL DEFAULT 0,
  `PlusBox` int(11) NULL DEFAULT 0,
  `Friends` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `Enemies` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `Me` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '[]',
  `ClanId` int(11) NULL DEFAULT -1,
  `LuckyBox` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `LastLogin` datetime(0) NULL DEFAULT '2022-03-05 18:25:21',
  `CreateDate` datetime(0) NULL DEFAULT '2022-03-05 18:25:21',
  `SpecialSkill` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `InfoBuff` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `diemsukien` int(11) NOT NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 10005 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of character
-- ----------------------------
INSERT INTO `character` VALUES (10002, 'admin', '[{\"Id\":0,\"SkillId\":0,\"CoolDown\":0,\"Point\":1}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":0,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":47,\"Param\":2}]},{\"IndexUI\":1,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":6,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":6,\"Param\":30}]},null,null,null,{\"IndexUI\":5,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":937,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":50,\"Param\":24},{\"Id\":77,\"Param\":20},{\"Id\":103,\"Param\":20},{\"Id\":94,\"Param\":10},{\"Id\":108,\"Param\":6},{\"Id\":80,\"Param\":15},{\"Id\":106,\"Param\":1},{\"Id\":186,\"Param\":1}]},null]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":457,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":4997,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]},{\"IndexUI\":1,\"SaleCoin\":1,\"BuyPotential\":0,\"Id\":194,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]},{\"IndexUI\":2,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":738,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":50,\"Param\":35},{\"Id\":77,\"Param\":30},{\"Id\":103,\"Param\":30},{\"Id\":154,\"Param\":1},{\"Id\":93,\"Param\":11},{\"Id\":73,\"Param\":1665873959}]},{\"IndexUI\":3,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":904,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":50,\"Param\":24},{\"Id\":77,\"Param\":50},{\"Id\":103,\"Param\":50},{\"Id\":80,\"Param\":20},{\"Id\":81,\"Param\":20},{\"Id\":30,\"Param\":0},{\"Id\":93,\"Param\":0},{\"Id\":73,\"Param\":1664923663}]},{\"IndexUI\":4,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":1,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":47,\"Param\":2},{\"Id\":30,\"Param\":0}]},{\"IndexUI\":5,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":2,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":47,\"Param\":3},{\"Id\":30,\"Param\":0}]},{\"IndexUI\":6,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":806,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":94,\"Param\":20},{\"Id\":108,\"Param\":10},{\"Id\":176,\"Param\":0}]},{\"IndexUI\":7,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":344,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":8,\"Reason\":\"\",\"Options\":[{\"Id\":82,\"Param\":20}]},{\"IndexUI\":8,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":444,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":6,\"Reason\":\"\",\"Options\":[{\"Id\":98,\"Param\":3}]},{\"IndexUI\":9,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":20,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":6,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]},{\"IndexUI\":10,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":17,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":7,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]},{\"IndexUI\":11,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":441,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":2,\"Reason\":\"\",\"Options\":[{\"Id\":95,\"Param\":5}]},{\"IndexUI\":12,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":446,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":3,\"Reason\":\"\",\"Options\":[{\"Id\":100,\"Param\":3}]},{\"IndexUI\":13,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":443,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":4,\"Reason\":\"\",\"Options\":[{\"Id\":97,\"Param\":5}]},{\"IndexUI\":14,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":445,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":5,\"Reason\":\"\",\"Options\":[{\"Id\":99,\"Param\":3}]},{\"IndexUI\":15,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":343,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":2,\"Reason\":\"\",\"Options\":[{\"Id\":83,\"Param\":20}]},{\"IndexUI\":16,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":345,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":4,\"Reason\":\"\",\"Options\":[{\"Id\":80,\"Param\":5}]},{\"IndexUI\":17,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":342,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":3,\"Reason\":\"\",\"Options\":[{\"Id\":81,\"Param\":5}]},{\"IndexUI\":18,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":19,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":5,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]},{\"IndexUI\":19,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":447,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":4,\"Reason\":\"\",\"Options\":[{\"Id\":101,\"Param\":5}]}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":12,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":14,\"Param\":1}]}]', '{\"NClass\":0,\"Gender\":0,\"MapId\":5,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":64,\"Bag\":-1,\"Level\":0,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":1200,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[-1,-1,-1,-1,-1],\"OSkill\":[0,-1,-1,-1,-1],\"CSkill\":0,\"CSkillDelay\":500,\"X\":968,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":100,\"OriginalMp\":100,\"OriginalDamage\":15,\"OriginalDefence\":0,\"OriginalCrit\":0,\"Hp\":130,\"Mp\":92,\"Stamina\":10000,\"MaxStamina\":10000,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":900760004,\"Diamond\":81495,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":true,\"PhukienPart\":-1,\"IsHavePet\":true,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":1664922290633,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '[66]', 0, 0, '[]', '[]', '{\"Id\":10002,\"Head\":950,\"Body\":951,\"Leg\":952,\"Bag\":-1,\"Name\":\"admin\",\"IsOnline\":false,\"Power\":1200}', -1, '[]', '2022-10-04 21:13:12', '2022-09-27 19:31:55', '{\"Id\":-1,\"Info\":\"Chưa có Nội Tại\nBấm vào để xem chi tiết\",\"Img\":5223,\"SkillId\":-1,\"Value\":0,\"nextAttackDmgPercent\":0,\"isCrit\":false}', '{\"ThucAnId\":-1,\"ThucAnTime\":0,\"CuongNo\":false,\"CuongNoTime\":0,\"BoHuyet\":false,\"BoHuyetTime\":0,\"BoKhi\":false,\"BoKhiTime\":0,\"GiapXen\":false,\"GiapXenTime\":0,\"AnDanh\":false,\"AnDanhTime\":0,\"MayDoCSKB\":false,\"MayDoCSKBTime\":0,\"CuCarot\":false,\"CuCarotTime\":0,\"BanhTrungThuId\":-1,\"BanhTrungThuTime\":0}', 0);
INSERT INTO `character` VALUES (10003, 'pocollo', '[{\"Id\":2,\"SkillId\":14,\"CoolDown\":0,\"Point\":1}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":1,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":47,\"Param\":2}]},{\"IndexUI\":1,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":7,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":6,\"Param\":20}]},null,null,null,null,null]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":457,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":15,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":12,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":14,\"Param\":1}]}]', '{\"NClass\":1,\"Gender\":1,\"MapId\":5,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":29,\"Bag\":-1,\"Level\":0,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":1200,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[-1,-1,-1,-1,-1],\"OSkill\":[2,-1,-1,-1,-1],\"CSkill\":2,\"CSkillDelay\":500,\"X\":1098,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":100,\"OriginalMp\":100,\"OriginalDamage\":15,\"OriginalDefence\":0,\"OriginalCrit\":0,\"Hp\":100,\"Mp\":92,\"Stamina\":10000,\"MaxStamina\":10000,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":2000000000,\"Diamond\":50000,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":false,\"PhukienPart\":-1,\"IsHavePet\":true,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":1664919297228,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '[79]', 0, 0, '[]', '[]', '{\"Id\":10003,\"Head\":29,\"Body\":10,\"Leg\":11,\"Bag\":-1,\"Name\":\"pocollo\",\"IsOnline\":false,\"Power\":1200}', -1, '[]', '2022-10-04 21:29:57', '2022-10-04 21:29:57', '{\"Id\":-1,\"Info\":\"Chưa có Nội Tại\nBấm vào để xem chi tiết\",\"Img\":5223,\"SkillId\":-1,\"Value\":0,\"nextAttackDmgPercent\":0,\"isCrit\":false}', '{\"ThucAnId\":-1,\"ThucAnTime\":0,\"CuongNo\":false,\"CuongNoTime\":0,\"BoHuyet\":false,\"BoHuyetTime\":0,\"BoKhi\":false,\"BoKhiTime\":0,\"GiapXen\":false,\"GiapXenTime\":0,\"AnDanh\":false,\"AnDanhTime\":0,\"MayDoCSKB\":false,\"MayDoCSKBTime\":0,\"CuCarot\":false,\"CuCarotTime\":0,\"BanhTrungThuId\":-1,\"BanhTrungThuTime\":0}', 0);
INSERT INTO `character` VALUES (10004, 'cadic', '[{\"Id\":4,\"SkillId\":28,\"CoolDown\":0,\"Point\":1}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":2,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":47,\"Param\":3}]},{\"IndexUI\":1,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":8,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":6,\"Param\":20}]},null,null,null,null,null]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":457,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":15,\"Reason\":\"\",\"Options\":[{\"Id\":73,\"Param\":0}]}]', '[{\"IndexUI\":0,\"SaleCoin\":0,\"BuyPotential\":0,\"Id\":12,\"BuyCoin\":0,\"BuyGold\":0,\"Quantity\":1,\"Reason\":\"\",\"Options\":[{\"Id\":14,\"Param\":1}]}]', '{\"NClass\":2,\"Gender\":2,\"MapId\":5,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":6,\"Bag\":-1,\"Level\":0,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":1200,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[-1,-1,-1,-1,-1],\"OSkill\":[4,-1,-1,-1,-1],\"CSkill\":4,\"CSkillDelay\":500,\"X\":983,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":100,\"OriginalMp\":100,\"OriginalDamage\":15,\"OriginalDefence\":0,\"OriginalCrit\":0,\"Hp\":100,\"Mp\":92,\"Stamina\":10000,\"MaxStamina\":10000,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":2000000000,\"Diamond\":50000,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":false,\"PhukienPart\":-1,\"IsHavePet\":true,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":1664919362883,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '[87]', 0, 0, '[]', '[]', '{\"Id\":10004,\"Head\":6,\"Body\":16,\"Leg\":17,\"Bag\":-1,\"Name\":\"cadic\",\"IsOnline\":false,\"Power\":1200}', -1, '[]', '2022-10-04 21:31:02', '2022-10-04 21:31:02', '{\"Id\":-1,\"Info\":\"Chưa có Nội Tại\nBấm vào để xem chi tiết\",\"Img\":5223,\"SkillId\":-1,\"Value\":0,\"nextAttackDmgPercent\":0,\"isCrit\":false}', '{\"ThucAnId\":-1,\"ThucAnTime\":0,\"CuongNo\":false,\"CuongNoTime\":0,\"BoHuyet\":false,\"BoHuyetTime\":0,\"BoKhi\":false,\"BoKhiTime\":0,\"GiapXen\":false,\"GiapXenTime\":0,\"AnDanh\":false,\"AnDanhTime\":0,\"MayDoCSKB\":false,\"MayDoCSKBTime\":0,\"CuCarot\":false,\"CuCarotTime\":0,\"BanhTrungThuId\":-1,\"BanhTrungThuTime\":0}', 0);

-- ----------------------------
-- Table structure for clan
-- ----------------------------
DROP TABLE IF EXISTS `clan`;
CREATE TABLE `clan`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `Name` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
  `Slogan` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
  `ImgId` int(11) NULL DEFAULT 0,
  `Power` bigint(20) NULL DEFAULT 0,
  `LeaderName` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
  `CurrMember` int(11) NULL DEFAULT 0,
  `MaxMember` int(11) NULL DEFAULT 10,
  `Date` bigint(20) NULL DEFAULT 0,
  `Level` int(11) NULL DEFAULT 1,
  `Point` int(11) NULL DEFAULT 0,
  `Members` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `Messages` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `CharacterPeas` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 1 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of clan
-- ----------------------------

-- ----------------------------
-- Table structure for disciple
-- ----------------------------
DROP TABLE IF EXISTS `disciple`;
CREATE TABLE `disciple`  (
  `id` int(11) NOT NULL,
  `Name` varchar(15) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NOT NULL DEFAULT '',
  `Status` int(11) NOT NULL DEFAULT 0,
  `Skills` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `ItemBody` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `InfoChar` longtext CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `CreateDate` datetime(0) NULL DEFAULT NULL,
  `Type` int(11) NULL DEFAULT 1,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of disciple
-- ----------------------------
INSERT INTO `disciple` VALUES (-10004, 'Đệ tử', 0, '[{\"Id\":2,\"SkillId\":14,\"CoolDown\":0,\"Point\":1}]', '[null,null,null,null,null,null,null]', '{\"NClass\":0,\"Gender\":1,\"MapId\":0,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":0,\"Bag\":-1,\"Level\":1,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":2000,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[],\"OSkill\":[],\"CSkill\":0,\"CSkillDelay\":500,\"X\":969,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":1277,\"OriginalMp\":1669,\"OriginalDamage\":53,\"OriginalDefence\":31,\"OriginalCrit\":4,\"Hp\":1277,\"Mp\":1669,\"Stamina\":1250,\"MaxStamina\":1250,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":0,\"Diamond\":0,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":false,\"PhukienPart\":-1,\"IsHavePet\":false,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":0,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '2022-10-04 21:31:02', 1);
INSERT INTO `disciple` VALUES (-10003, 'Đệ tử', 0, '[{\"Id\":2,\"SkillId\":14,\"CoolDown\":0,\"Point\":1}]', '[null,null,null,null,null,null,null]', '{\"NClass\":0,\"Gender\":1,\"MapId\":0,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":0,\"Bag\":-1,\"Level\":1,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":2000,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[],\"OSkill\":[],\"CSkill\":0,\"CSkillDelay\":500,\"X\":1048,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":2162,\"OriginalMp\":1977,\"OriginalDamage\":45,\"OriginalDefence\":37,\"OriginalCrit\":1,\"Hp\":2162,\"Mp\":1977,\"Stamina\":1250,\"MaxStamina\":1250,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":0,\"Diamond\":0,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":false,\"PhukienPart\":-1,\"IsHavePet\":false,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":0,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '2022-10-04 21:29:57', 1);
INSERT INTO `disciple` VALUES (-10002, 'Đệ tử', 0, '[{\"Id\":4,\"SkillId\":28,\"CoolDown\":0,\"Point\":1}]', '[null,null,null,null,null,null,null]', '{\"NClass\":0,\"Gender\":2,\"MapId\":0,\"MapCustomId\":-1,\"ZoneId\":0,\"Hair\":0,\"Bag\":-1,\"Level\":0,\"Speed\":6,\"Pk\":0,\"TypePk\":0,\"Potential\":0,\"TotalPotential\":0,\"Power\":2000,\"IsDie\":false,\"IsPower\":true,\"LitmitPower\":16,\"KSkill\":[],\"OSkill\":[],\"CSkill\":0,\"CSkillDelay\":500,\"X\":963,\"Y\":408,\"HpFrom1000\":20,\"MpFrom1000\":20,\"DamageFrom1000\":1,\"Exp\":100,\"OriginalHp\":2127,\"OriginalMp\":1292,\"OriginalDamage\":43,\"OriginalDefence\":28,\"OriginalCrit\":1,\"Hp\":2127,\"Mp\":1292,\"Stamina\":1250,\"MaxStamina\":1250,\"NangDong\":0,\"MountId\":-1,\"Teleport\":1,\"Gold\":0,\"Diamond\":0,\"DiamondLock\":0,\"LimitGold\":2000000000,\"LimitDiamond\":2000000000,\"LimitDiamondLock\":2000000000,\"IsNewMember\":true,\"IsNhanBua\":false,\"PhukienPart\":-1,\"IsHavePet\":false,\"IsPremium\":false,\"ThoiGianTrungMaBu\":0,\"TimeAutoPlay\":0,\"CountGoiRong\":0,\"Fusion\":{\"IsFusion\":false,\"IsPorata\":false,\"IsPorata2\":false,\"TimeStart\":-1,\"DelayFusion\":-1,\"TimeUse\":0},\"LockInventory\":{\"IsLock\":false,\"Pass\":-1,\"PassTemp\":-1},\"Task\":{\"Id\":24,\"Index\":0,\"Count\":0},\"LearnSkill\":null,\"LearnSkillTemp\":null,\"ItemAmulet\":{},\"Cards\":{},\"TrainManhVo\":0,\"TrainManhHon\":0,\"SoLanGiaoDich\":0,\"ThoiGianGiaoDich\":0,\"ThoiGianChatTheGioi\":0,\"ThoiGianDoiMayChu\":0,\"HieuUngDonDanh\":true,\"EffectAuraId\":-1,\"PetId\":-1,\"PetImei\":-1}', '2022-09-27 19:31:55', 1);

-- ----------------------------
-- Table structure for gameinfo
-- ----------------------------
DROP TABLE IF EXISTS `gameinfo`;
CREATE TABLE `gameinfo`  (
  `id` int(11) NULL DEFAULT NULL,
  `main` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  `content` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of gameinfo
-- ----------------------------
INSERT INTO `gameinfo` VALUES (0, 'SUPER HERO', 'Chào mừng bạn đến với máy chủ test Ngọc Rồng SUPER HERO');
INSERT INTO `gameinfo` VALUES (1, 'Máy chủ NRO SUPER HERO đang trong quá trình thử nghiệm', 'Máy chủ NRO SUPER HERO đang trong quá trình thử nghiệm');

-- ----------------------------
-- Table structure for giftcode
-- ----------------------------
DROP TABLE IF EXISTS `giftcode`;
CREATE TABLE `giftcode`  (
  `code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `count` int(11) NULL DEFAULT 1,
  `time_expire` datetime(0) NULL DEFAULT NULL,
  `type` int(11) NULL DEFAULT 0,
  PRIMARY KEY (`code`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of giftcode
-- ----------------------------
INSERT INTO `giftcode` VALUES ('kyuctanthu', 999, '2022-10-27 20:56:37', 1);

-- ----------------------------
-- Table structure for giftcode_used
-- ----------------------------
DROP TABLE IF EXISTS `giftcode_used`;
CREATE TABLE `giftcode_used`  (
  `code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `character` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `time_used` datetime(0) NULL DEFAULT NULL,
  `type` int(11) NULL DEFAULT NULL,
  PRIMARY KEY (`code`, `character`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of giftcode_used
-- ----------------------------

-- ----------------------------
-- Table structure for magictree
-- ----------------------------
DROP TABLE IF EXISTS `magictree`;
CREATE TABLE `magictree`  (
  `id` bigint(20) NOT NULL AUTO_INCREMENT,
  `idNpc` int(11) UNSIGNED NOT NULL DEFAULT 0,
  `x` int(11) NULL DEFAULT 0,
  `y` int(11) NULL DEFAULT 0,
  `level` int(11) NULL DEFAULT 1,
  `peas` int(11) NULL DEFAULT 5,
  `maxPea` int(11) NULL DEFAULT 5,
  `seconds` bigint(20) NULL DEFAULT 0,
  `isUpdating` int(11) NULL DEFAULT 0,
  `Diamond` int(11) NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 10005 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of magictree
-- ----------------------------
INSERT INTO `magictree` VALUES (10002, 84, 348, 336, 1, 5, 5, 0, 0, 0);
INSERT INTO `magictree` VALUES (10003, 371, 372, 336, 1, 5, 5, 0, 0, 0);
INSERT INTO `magictree` VALUES (10004, 378, 372, 336, 1, 5, 5, 0, 0, 0);

-- ----------------------------
-- Table structure for napthe
-- ----------------------------
DROP TABLE IF EXISTS `napthe`;
CREATE TABLE `napthe`  (
  `callback_sign` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NOT NULL,
  `status` int(11) NULL DEFAULT NULL COMMENT '0 đang chờ, 1 thành công, 2 lỗi',
  `request_id` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `telco` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `serial` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `code` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `trans_id` bigint(20) NULL DEFAULT NULL,
  `value` int(11) NULL DEFAULT NULL COMMENT 'Giá trị thực của thẻ',
  `message` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci NULL DEFAULT NULL,
  `declared_value` int(11) NULL DEFAULT NULL COMMENT 'Số tiền gửi lên',
  `amount` int(11) NULL DEFAULT NULL COMMENT 'Giá trị thực nhận',
  `response_code` int(11) NULL DEFAULT NULL COMMENT 'Giá trị trả về khi gửi thẻ',
  `created_time` datetime(0) NOT NULL DEFAULT CURRENT_TIMESTAMP(0),
  `updated_time` datetime(0) NULL DEFAULT NULL ON UPDATE CURRENT_TIMESTAMP(0),
  PRIMARY KEY (`callback_sign`) USING BTREE
) ENGINE = InnoDB CHARACTER SET = utf8mb4 COLLATE = utf8mb4_general_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of napthe
-- ----------------------------

-- ----------------------------
-- Table structure for regexchat
-- ----------------------------
DROP TABLE IF EXISTS `regexchat`;
CREATE TABLE `regexchat`  (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `text` varchar(500) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  PRIMARY KEY (`id`) USING BTREE,
  UNIQUE INDEX `id`(`id`) USING BTREE,
  INDEX `id_2`(`id`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 24 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of regexchat
-- ----------------------------
INSERT INTO `regexchat` VALUES (1, 'địt');
INSERT INTO `regexchat` VALUES (2, 'đm');
INSERT INTO `regexchat` VALUES (3, 'dm');
INSERT INTO `regexchat` VALUES (4, 'lồn');
INSERT INTO `regexchat` VALUES (5, 'dmm');
INSERT INTO `regexchat` VALUES (6, 'dcmm');
INSERT INTO `regexchat` VALUES (7, 'djt');
INSERT INTO `regexchat` VALUES (8, 'dit');
INSERT INTO `regexchat` VALUES (15, 'dell');
INSERT INTO `regexchat` VALUES (16, 'deo');
INSERT INTO `regexchat` VALUES (19, 'admin');
INSERT INTO `regexchat` VALUES (20, 'assmin');
INSERT INTO `regexchat` VALUES (21, 'clmm');
INSERT INTO `regexchat` VALUES (22, 'cltx');
INSERT INTO `regexchat` VALUES (23, 'cl');

-- ----------------------------
-- Table structure for user
-- ----------------------------
DROP TABLE IF EXISTS `user`;
CREATE TABLE `user`  (
  `id` int(10) UNSIGNED NOT NULL AUTO_INCREMENT,
  `username` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
  `password` varchar(50) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT '',
  `character` bigint(20) NULL DEFAULT 0,
  `lock` tinyint(4) NULL DEFAULT 0,
  `role` int(11) NULL DEFAULT 0,
  `ban` tinyint(4) NULL DEFAULT 0,
  `online` tinyint(4) NULL DEFAULT 0,
  `created_at` timestamp(0) NULL DEFAULT NULL,
  `updated_at` timestamp(0) NULL DEFAULT NULL,
  `sdt` text CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL,
  `vnd` int(11) NOT NULL DEFAULT 0,
  `tongnap` int(11) NOT NULL DEFAULT 0,
  `email` varchar(255) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  `diemtichnap` int(11) NOT NULL DEFAULT 0,
  `sv_port` int(11) NOT NULL DEFAULT 14445,
  `logout_time` bigint(20) NOT NULL DEFAULT 0,
  `last_ip` varchar(24) CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci NULL DEFAULT NULL,
  `is_login` tinyint(4) NULL DEFAULT 0,
  PRIMARY KEY (`id`) USING BTREE,
  INDEX `character`(`character`) USING BTREE
) ENGINE = InnoDB AUTO_INCREMENT = 25096 CHARACTER SET = utf8mb4 COLLATE = utf8mb4_unicode_ci ROW_FORMAT = DYNAMIC;

-- ----------------------------
-- Records of user
-- ----------------------------
INSERT INTO `user` VALUES (25089, 'admin1', 'xaxaxa', 10002, 0, 1, 0, 0, NULL, NULL, NULL, 0, 20000, NULL, 0, 14445, 1664922463, '127.0.0.1', 0);
INSERT INTO `user` VALUES (25094, 'admin2', 'xaxaxa', 10003, 0, 1, 0, 0, NULL, NULL, NULL, 5000000, 5000000, NULL, 0, 14445, 1664919086, '127.0.0.1', 0);
INSERT INTO `user` VALUES (25095, 'admin3', 'xaxaxa', 10004, 0, 1, 0, 0, NULL, NULL, NULL, 5000000, 5000000, NULL, 0, 14445, 1664920834, '127.0.0.1', 0);

SET FOREIGN_KEY_CHECKS = 1;
