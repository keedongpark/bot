# 설명 

 Compile을 할 때 마다 수십 메가 메모리 증가. 
 별도의 VM 생성 등이 이루어지는 것으로 보임. 
 
 RunAsync를 매번 하면 느린 문제가 있음 

# 목표 

 스크립트 사용은 유지하면서 효율적으로 실행할 방버을 찾아야 함 

 방향 : 
 - Compile() 결과를 캐싱하지 않고 사용. 
   - 메모리 증가는 있지만 다소 감소함 
   - 별로 좋은 방법은 아닌 듯
 - 전체 스크립트를 묶어서 빌드하고 함수 호출로 처리
   - C# 언어를 좀 더 많이 알아야 한다.  
   - gamein_on(), req_login()
   - static class 함수로 생성해서 처리 가능 
   - 호출은 reflection을 사용해야 함 
   - 이 방향으로 진행한다. 
 - ScriptEngine 사용 
   - https://gist.github.com/amazedsaint/3828951
   - 이 쪽은 괜찮을 것 같다. 
 - http://www.csscript.net/   
   - 진짜 C# 스크립트를 사용하는 방법. 
   - 성능이 이 쪽이 더 나을 수도 있다. 


# 리서치 

 - Compile 할 때 생성되는 것들 

 https://keestalkstech.com/2016/05/how-to-add-dynamic-compilation-to-your-projects/
 - IScript의 assembly 생성 및 실행 

https://stackoverflow.com/questions/32769630/how-to-compile-a-c-sharp-file-with-roslyn-programmatically
 - CSharpCompilation.Emit() 
 - dll 생성 방식. 

https://joshvarty.wordpress.com/2015/10/15/learn-roslyn-now-part-14-intro-to-the-scripting-api/
 - ScriptHost() argument 
 

 # 정리 
  
  매번 컴파일 하는 건 느리기 때문에 모아서 빌드하고 함수 호출을 사용하는 것으로 정리 

