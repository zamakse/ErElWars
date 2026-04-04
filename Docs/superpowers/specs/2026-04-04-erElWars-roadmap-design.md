# 에르엘워즈 (ErElWars) — 개발 로드맵 & 아키텍처 설계

**작성일:** 2026-04-04
**엔진:** Unity (C#) · URP 2D
**플랫폼:** Android 우선, 이후 iOS
**수익화:** 광고 기반 F2P (AdMob / Unity Ads)
**아트:** AI 생성 이미지 활용
**팀:** 솔로 개발
**목표 출시:** 1년 이상

---

## 1. 프로젝트 목표

에르엘워즈2(AREL WARS 2, 게임빌 2012)를 레퍼런스로 삼는 2D 횡스크롤 공격형 라인 전략 게임.
플레이어가 마나를 소비해 병력을 소환하고, 자동 전투로 상대 기지를 파괴하는 것이 목표.

**성공 기준:**
- Google Play 출시 후 평점 4.0+ 유지
- 핵심 전투 루프가 모바일 터치로 직관적으로 동작
- 75스테이지 + 하드모드로 충분한 플레이 볼륨 제공
- 광고 수익 구조가 게임플레이를 방해하지 않는 수준

---

## 2. 개발 로드맵 (마일스톤 기반)

### M1 — 전투 루프 완성 (~2개월)

**완료 기준:** 유닛을 뽑으면 자동 전진 → 충돌 → 전투 → 기지 공격 → 승패 결정. 적도 자동 스폰.

| 작업 | 파일 | 상태 |
|------|------|------|
| UnitMover — 자동 전진, 적 앞에서 멈춤 | `Unit/UnitMover.cs` | 진행중 |
| UnitCombat — 공격·피격·사망·상성 적용 | `Unit/UnitCombat.cs` | 진행중 |
| BaseHP — 기지 HP·승패 판정 | `Battle/BaseHP.cs` | 진행중 |
| EnemyAI — 스테이지 데이터 기반 자동 스폰 | `AI/EnemyAI.cs` | 스텁 |
| BattleUI — 마나 게이지·유닛 버튼·HP바 (임시 UI) | `UI/BattleUI.cs` | 스텁 |
| ProjectileManager — 원거리 투사체 | `Battle/ProjectileManager.cs` | 스텁 |
| UnitAir — 공중 유닛 전용 로직 | `Unit/UnitAir.cs` | 스텁 |

### M2 — 게임 루프 완성 (~3개월)

**완료 기준:** 영웅 출진 → 스킬 사용 → 스테이지 5개 클리어 가능한 플레이어블 빌드.

- HeroBase / HeroStats (STR/CON/DEX/INT) / HeroSkill (4슬롯) / HeroRevive
- BattleUI 완성 (슬롯 8개 + 영웅 상태창 + 미니맵)
- StageManager — 스테이지 로드·클리어·해금
- 스테이지 5개 제작 (테마 1)
- 버스터 종족 유닛 완전 구현 (유닛 4~5종 + 영웅 플로라)

### M3 — 콘텐츠 확장 (~3개월)

**완료 기준:** 3종족 플레이 가능, 스테이지 30개, 강화 시스템 동작.

- 데븐·유니언 종족 추가 (유닛 + 영웅 각 1명)
- FactionManager — 종족 선택 화면
- UnitEnhancer (강화) / UnitFrenzy (광폭) / HeroEquipment (장비)
- WorldMapUI — 노드 기반 월드맵
- 스테이지 누적 30개 (테마 1~2 완성 + 테마 3 일부)
- AI 생성 아트 1차 통합 (유닛 스프라이트·배경)

### M4 — 출시 준비 (~3개월)

**완료 기준:** 스토어 제출 가능한 빌드. 광고 동작, 저장 동작.

- 스테이지 75개 완성 (테마 3 마무리 + 테마 4~5) + 하드 모드
- AdManager — Google AdMob 통합 (전면 광고 + 보상형 광고)
- SaveManager — 진행 저장·로드 (PlayerPrefs 또는 클라우드)
- AudioManager — BGM / SFX
- 아트 최종 통합 (UI 포함)
- Android 빌드 최적화 + 기기 호환성 QA
- Google Play 스토어 등록

### M5 — 폴리싱 & iOS (~1개월+)

**완료 기준:** iOS 출시, 튜토리얼 완비, 첫 패치 반영.

- 밸런스 시트 기반 전체 수치 조정
- 튜토리얼 시스템
- 오브젝트 풀링 등 성능 최적화
- iOS 빌드 + App Store 등록
- 플레이어 피드백 반영

---

## 3. 시스템 아키텍처

### 레이어 구조 (단방향 의존)

```
Core (GameManager / StageManager / FactionManager)
  ↓ 전투 시작/종료 신호
Battle (BattleManager / LineManager / UnitSpawner / EnemyAI / ManaManager / ProjectileManager)
  ↓ 유닛 인스턴스 생성·제어
Unit (UnitBase / UnitMover / UnitCombat / UnitAir / BaseHP)
  ↓ 상속
Hero (HeroBase / HeroStats / HeroSkill / HeroRevive / HeroEquipment)
  ↓ 수치 읽기 / 스탯 수정
Data/Upgrade (UnitData / HeroData / StageData / UnitEnhancer / UnitFrenzy / AffinitySystem)
  ↓ 이벤트(OnManaChanged 등)로만 수신
UI (BattleUI / WorldMapUI / HeroStatusUI)
  ↓ M4에서 추가
External (AdManager / SaveManager / AudioManager)
```

### 핵심 설계 원칙

1. **단방향 의존** — 상위 레이어가 하위를 호출, 역방향 없음
2. **UI는 이벤트만 수신** — `OnManaChanged`, `OnDamaged` 등 이벤트로만 상태 수신. UI 코드에서 로직 직접 호출 금지
3. **수치는 ScriptableObject/JSON만** — 코드에 숫자 하드코딩 없음. 유닛·영웅·스테이지 수치는 모두 에셋 파일에서 관리
4. **싱글턴은 매니저급만** — BattleManager, ManaManager, LineManager 등 전역 매니저만 싱글턴. 유닛 개체는 싱글턴 금지

### 현재 구현 상태

| 시스템 | 상태 |
|--------|------|
| GameTypes (LineType / Faction / UnitType / AffinitySystem) | 완료 |
| ManaManager | 완료 |
| LineManager | 완료 |
| UnitSpawner | 완료 |
| UnitBase | 완료 |
| BattleManager | 기본 완료 |
| UnitMover, UnitCombat, BaseHP | 진행중 |
| EnemyAI, Hero 시스템, UI, Upgrade | 스텁 |
| AdManager, SaveManager, AudioManager | 미착수 |

---

## 4. M1 빌드 순서 (당장 시작할 작업)

1. **UnitMover 완성** — 전진 이동 + 적 앞에서 멈춤
2. **UnitCombat 완성** — 공격·피격·사망·AffinitySystem 연결
3. **BaseHP 완성** — 기지 공격·HP 감소·BattleManager 승패 연결
4. **EnemyAI 스폰 로직** — 일정 간격 적 유닛 자동 스폰
5. **BattleUI 기본** — 마나 게이지·유닛 생산 버튼 (임시 UI 가능)
6. **ProjectileManager** — 원거리 투사체 기본 동작

각 단계는 이전 단계 완료 후 진행. 1~3번이 완료되면 적 AI 없이도 수동 테스트 가능.

---

## 5. AI 아트 파이프라인

### 권장 워크플로

1. **생성** — Midjourney / DALL-E / Stable Diffusion으로 이미지 생성
   - 유닛: 정면·측면 각도, 투명 배경 (PNG)
   - 배경: 2048×1024 가로형 레이어 분리 (원경·중경·전경)
2. **후처리** — Remove.bg 또는 Photoshop으로 배경 제거
3. **스프라이트 설정** — Unity에서 Pixels Per Unit 통일 (권장: 100), Sprite Mode = Single
4. **애니메이션** — Spine2D 또는 Unity Animator로 Idle/Walk/Attack/Die 4컷 이상

### 에셋 명명 규칙

| 종류 | 규칙 | 예시 |
|------|------|------|
| 유닛 스프라이트 | `sprite_{종족}_{유닛명}_{상태}` | `sprite_buster_warrior_idle` |
| 영웅 스프라이트 | `sprite_hero_{영웅명}_{상태}` | `sprite_hero_flora_attack` |
| 배경 레이어 | `bg_{테마번호}_{레이어}` | `bg_01_far` |
| UI 요소 | `ui_{이름}` | `ui_mana_bar` |

---

## 6. 광고 통합 전략 (M4)

- **전면 광고** — 스테이지 클리어 후 표시 (5스테이지마다 1회)
- **보상형 광고** — 선택적 시청으로 보상 지급
  - 영웅 즉시 부활 (사망 후 30초 대기 스킵)
  - 마나 30% 즉시 충전
  - 스테이지 실패 후 1회 부활
- **원칙** — 광고를 강제하지 않음. 보상형만 적극 활용, 플레이 흐름을 끊는 팝업 최소화

---

## 7. 저장 시스템 설계 (M4)

저장 항목:
- 현재 종족·영웅 선택
- 스테이지 클리어 기록 (일반·하드 각각)
- 유닛 강화·광폭 수치
- 영웅 레벨·스탯 분배·장비
- 보유 골드·누적 광고 시청 횟수

구현: `PlayerPrefs` (로컬) → M5에서 Google Play 클라우드 세이브 검토
