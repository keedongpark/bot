# Flow Enhance

재사용이 핵심. 

쉽게 작성하는 것도 중요하지만 반복하지 않는 게 더 중요. 

acts 단위를 파일에 정의하고 재사용

acts: 
  use: mod_login.connect
  use: mod_login.login 
  act: 
     do: > 
  use: mod_login.logout
     

