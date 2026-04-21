# AI-Tetris
여러 AI툴을 활용해 테트리스를 제작하는 AI 연습 프로젝트입니다.

<details>
  <summary>참고 및 요구 사항</summary>
   
    저는 Unity 프로젝트를 진행하고 있습니다.
    
    테트리스를 만들어주세요.
    구현 참고는 다음과 같습니다.

    ## 참고 사항
    - {name}: Codex
    - Git Repository URL: https://github.com/taku7664/AI-Tetris
    - Unity 버전: 6000.4.1f1
    - InputAction 패키지 설치 및 활성화 완료
    - TextMeshPro 패키지 설치 완료
    
    ## 규칙
    
    ### 네이밍 규칙
    - 모든 파일의 경로는 ./Assets/{name}/ 를 사용합니다.
    - 모든 파일명은 {name}_ 헤더를 붙입니다.
    - 모든 객체는 {name} 네임스페이스를 사용합니다.
    - 데이터는 헤더와 상관없이 작명합니다. 다만 변수의 경우 PascalCase를 사용합니다.
    
    ### 커밋 규칙
    - Github를 사용하는 경우 브랜치는 main브랜치가 아닌 main브랜치에서 파생한 {name} 브랜치를 생성해 해당 브랜치에서 작업합니다.
    - .ignore파일 규격에 맞는 파일만 커밋합니다.
    - 한번에 커밋하지 않고 작업 분기별로 커밋합니다.
    - 기능 추가의 작업인 경우 Commit 시 Summary에 feat: 헤더를 붙입니다.
    - 버그 수정의 작업인 경우 Commit 시 Summary에 fix: 헤더를 붙입니다.
    - 잡다한 수정의 작업인 경우 Commit 시 Summary에 chore: 헤더를 붙입니다.
    
    ### 작업 규칙
    - ./Assets 내에서만 작업하며, 불필요한 파일을 읽지 않습니다.
    - Git Repository 권한이 낮아 Branch 생성과 Commit에 문제가 있다면 다시 요청하세요.
    - 유효한 경로를 찾지 못하거나 이해하지 못한 부분이 있으면 작업하지 말고 중단한 후 필요한 정보를 다시 요청합니다.
    - 신규 파일을 만들 때 .meta파일을 직접 생성해서 에셋 참조 시 해당 GUID를 사용합니다.
    
    ## 내부 씬 구조
    - FieldBox: 실제 테트리스 화면이 나올 영역(Scale 참고)
    - PreviewBox: 다음 블럭이 출력될 영역(Scale 참고)
    - Score: 스코어가 출력되고 있는 텍스트
    - GameManager: FieldBox, PreviewBox를 GameObject로, Score를 Tmp_Text로 들고있음. (바인딩되어있는 상태)
    
    ## 요구 사항
    
    ### 데이터 요구 사항
    GameManager에서 다음과 같은 Serialize Field가 제공되어야합니다.
      1) 필드의 x칸, y칸 int (default value: x:10, y:20)
      2) 블럭이 한칸 내려오는 타이머 float per second (default value: 0.5)
      3) 줄을 제거할 때 마다 추가 점수 List per line (default value: 1:100, 2:300, 3:500, 4:800)
    
    ### 게임 뷰 관련 요구 사항
    - PreviewBox 내부에 다음 블럭이 출력(렌더링)되야합니다.
    - FieldBox 내부에 현재 테트리스 게임이 출력(렌더링)되야합니다.
    - FieldBox 내부에 실제 Available 영역의 검은색 Rect가 출력(렌더링)되야합니다.
    - PreviewBox 내부 블록 칸은 정사각형이어야 합니다.
    - GameManager의 필드 칸 수가 늘거나 줄어도 총 필드 크기가 FieldBox의 크기를 넘지 않는 선에서 균일해야 합니다.
    - 현재 블록이 도착했을 때 어디에 놓일지 미리 보여줘야합니다. 이 블록은 0.2의 알파 값을 가집니다.
    
    ### Input 관련 요구 사항
    - Input API는 InputAction을 사용합니다.
    - 인풋 액션 에셋은 ./Assets/{name}/ 폴더에 별도로 만듭니다.
    - 인풋 트리거가 Pressed라고 명시되어 있으면 누를 때만 동작하게 작업합니다.
    - 인풋 트리거가 Press라고 명시되어 있으면 반복적인 refeat반응을 필요로 하는 것입니다.
    - SpaceBar(pad: A)키를 Pressed하면 현재 블럭이 바로 내려와야합니다.
    - ↓(pad: dpad)키를 Press하면 블럭이 빠르게 내려와야합니다.
    - ↑(pad: dpad)키를 Pressed하면 블럭이 시계방향으로 90도 회전해야합니다.
    - →, ←(pad: dpad)키를 Press하면 방향에 맞게 블럭이 한칸 이동해야합니다.
    - Enter(pad: start)키를 Pressed하면 재시작을 해야합니다.
    - 키보드와 게임패드를 지원해야 합니다.
    
    ### 그 외 요구 사항
    - ./Assets/ 경로 에 있는 ReferenceScene.unity과 GameManager.cs의 내용을 참조하여, ./Assets/{name}/ 폴더에 {name}_Scene.unity와 {name}_GameManager.cs를 만든 후 작업합니다. 이 때, 오브젝트 바인딩 구조와 내용도 그대로 참조합니다.
    - 블럭은 단면 테트로미노를 사용해 7가지를 제작합니다.
    - 놓인 블럭중에 최대 높이가 필드의 y칸을 초과하는 경우 GameOver가 되야합니다.
    - GameOver 시 별도의 UI 없이 게임을 멈추고 재시작 입력만을 받습니다.
    - 재시작이 가능해야합니다.
    - 블럭이 바닥에 닿자마자 바로 설치되지 않고 블럭 타이머 시간 만큼 뒤에 지연 설치되야합니다.
    
    ### 블록 색상 요구 사항
    | 블록 |    색상명 |               RGB |       Hex |
    | -- | -----: | ----------------: | --------: |
    | I  |   Cyan | **(0, 255, 255)** | `#00FFFF` |
    | O  | Yellow | **(255, 255, 0)** | `#FFFF00` |
    | T  | Purple | **(128, 0, 128)** | `#800080` |
    | S  |  Green |   **(0, 255, 0)** | `#00FF00` |
    | Z  |    Red |   **(255, 0, 0)** | `#FF0000` |
    | J  |   Blue |   **(0, 0, 255)** | `#0000FF` |
    | L  | Orange | **(255, 165, 0)** | `#FFA500` |
    
    ## 다음과 같은 설계를 하면 좋습니다!
    - 객체 단위의 스크립트 분리
    - Object Pool 사용
    
    ## 완료 시 다음과 유의미한 성과가 있다면 보고해주세요!
    1) 확장성 부분
    2) 최적화 부분
    3) 객체 지향적 설계 부분
    
    못한 부분이 있거나 부족한 정보가 있으면 말해주세요.
    이해했다면 다음 질문에 답해주세요.
    1) Git 커밋 권한이 있는지 여부
    2) 임의의 커밋 메세지 출력
    3) 파일을 생성할 때 어떻게 생성할 것인지 임의로 출력
    4) class 작성 시 어떻게 작성할 것인지 임의로 출력
    5) 브랜치 생성 경우 어떤 경로로 만들 것인지 출력
    또한, ReferenceScene.unity와 GameManager.cs 파일을 확인 후, 확인 여부를 출력해주세요.
    
</details>

<details>
    <summary>작업 절차 수립 및 시행</summary>
    
    부족한 정보가 없다면 다음과 같은 작업을 진행해주세요.
    1) Git Repository 권한 확인 및 Publish 브랜치 생성
    2) 기본 Scene과 GameManager 파일 생성 후 커밋
    3) 구조 설계 및 계획
    4) 계획에 따른 단위 작업 + 커밋
    5) 결과 보고
    작업 도중 오류나 부족한 정보가 발생한다면 작업을 중단하고 저에게 요청하세요.

</details>

# Chat GPT - 완료
<img width="400" height="550" alt="image" src="https://github.com/user-attachments/assets/c5f042b9-a899-4282-80e3-f798223d842b" />
<img width="400" height="550" alt="image" src="https://github.com/user-attachments/assets/051a8c65-930a-4b71-ba2b-66ff74aab380" />

# VS Code - 완료
<img width="405" height="543" alt="image" src="https://github.com/user-attachments/assets/8a4ebf36-677d-488d-a745-b50e4cb36cf2" />
<img width="405" height="543" alt="image" src="https://github.com/user-attachments/assets/673c3bd7-2962-476e-a924-76be118ddf32" />

# Copilot - 미완료
<img width="306" height="396" alt="image" src="https://github.com/user-attachments/assets/e4608638-2b98-4999-84d7-726cc100e73b" />

