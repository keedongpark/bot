# Flow 

 - Flow of Acts with expected result 
 - Flow runs under an Agent context 
 - Act is a heart of Flow 

 Agent와 Act는 게임별 factory에서 생성. 
 json 정의 파일 로딩 후 각 에이전트에서 스크립트 오브젝트 생성 (미리 컴파일)
 스크립트에 전달되는 agent, msg는 각 게임과 해당하는 메세지로 처리.

 디버깅 쉽게 되도록 하는 것. 
 각 게임들 쉽게 정의 되도록 하는 것이 중요. 

## Act 

 - script 실행 
 - act timeout 처리 
 - script 정의 재사용 기능 
 - flow는 개념으로만 남김 (act의 하위 클래스)
 - act가 act들을 포함할 수 있도록 함  
  
# 스크립트로 전체 정의

 - login: 

        flow : { 
            acts : [
                { act: "cond", do: "!agent.is_logined() && agent.is_connected()" }, 
                { act: "req",  do: "agent.send_login()" }, 
                { act: "wait", do: "agent.wait_msg("", 5)"}, 
                { act: "cond", do: "agent.is_logined()" }                
            ]
        }

 - battle: 

        flow: { 
            acts: [
                { act: "cond",        do: "agent.is_logined() && agent.is_connected()" }, 
                { act: "change_map",  
                    do: [
                        "args["map_id"] = 3", 
                        "args["timeout"] = 5", 
                        "agent.run_act("change_map"));", 
                        "comment": "change_map is called with args set"
                    ]                     
                },                                
            ]
        }

 - function flow / act 

        flow: { 
            args: [map_id, timeout], # for description

            act: "change_map", 
            
            acts: [
                { act: "cond", do: "agent.is_logined()" }, 
                { act: "req", do: "agent.send_changemap(args["map_id"], args["timeout"])" },
                { act: "wait", timeout: "5", do: "agent.wait_msg("")"}
            ]
        }

## 테스트 한 내용 

 Roslyn 컴파일러 설치해서 테스트 

  - expr / 함수 호출 모두 가능 
  - script로 변수 (agent, msg) 전달 가능 
  - script 사전 컴파일 및 사용 가능 
    - 성능은 최초 컴파일이 좀 느리나 사용 자체는 괜찮음 
  - reflection 통해 함수 호출 잘 됨 
    - argument가 object로 전달되므로 쉽게 전달 가능 

## yaml로 변경 

 json이 이런 구성을 하기에는 verbosity가 높다. 
 여러 라인에 코드를 작성할 때도 불편한 점들이 있다.

 http://serious-code.net/doku/doku.php?id=kb:yamltutorial
 - 기본 사항들이 잘 설명되어 있음 

 여러 라인에 걸치는 문자열
 - Block scalar styles (>, |)
 - >는 라인문자를 공백으로 교체 (폴딩)
 - |는 그대로 유지함 (블록) 

C# 코드 문법 체크가 일반적인 에디터에서 안 됨. 


## exec, get, set 함수 

reflection을 사용해서 exec, get, set 함수를 구현. 
 
### yaml / exec / get / set 버전 

get/set의 필드는 정의가 없으면 dictionary를 사용.

    - flow: 
        name: login 
        acts: 
            - act: 
                name: check_connected
                type: cond 
                do: agent.is_connected() 

            - act: 
                type: exec
                do: agent.exec(""send_login"", parms)

            - act: 
                type: wait_msg
                timeout: 3
                do: > 
                if ( msg.get("key") == ""login_resp"" )
                {
                    if ( msg.get(""result"") == true )
                    {
                        agent.set(""is_logined"", true);
                    }
                    else 
                    {
                        agent.fail();
                    }
                }

            - act: 
                type: cond
                do: > 
                    agent.get("is_logined")


### msg 

 json에서 필요한 값들 추출해서 유지하고 직접 접근 가능하게 해줌 

        msg["id"]["value"]

c# indexer를 사용해서 키 값으로 접근 가능하도록 함. 

#### 이전 메세지의 참조 

agent에 일일이 구현하기 어려울 수 있어 메세지를 보관하고 참조하면 
편리한 경우가 많이 있다. 

이런 경우를 대비하여 기능 추가 고려


### act 재사용 

parameter를 넘길 수 있도록 하고 재사용 가능하게 만듦 
참조는 flows["login"]["change_map"] 형태로 참조 가능하게 만듦 

