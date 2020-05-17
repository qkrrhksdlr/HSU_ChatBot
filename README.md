# HSU_ChatBot
 한성대학교 챗봇 (거북챗)
 
 <자세한 설명은 거북챗_개요.pdf과 거북챗_포트서.pdf를 참고하시기 바랍니다.>
 
 # <개발배경>
 - 한성대학교 홈페이지에서 원하는 정보를 찾고자 할 때 쉽게 찾을 수 없어서 힘든 경험이 많다.
 - 한성대학교 후배들에게 편리하고 재미있는 유익한 서비스를 제공하고자 개발함.
 - 타 대학(성균관대, 단국대 등)에서 사용되는 챗봇의 편리함을 보고, 
   한성대에도 챗봇이 있으면 어떨까? 라는 생각으로 출발함.
 
 # <작품개요>
 - 필요한 정보를 문장으로 요청할 수 있는 챗봇
 - 자연어 처리를 통하여 대화의도와 개체 학습을 통한 사용자의 정확한 의도를 추출한다.
 - 한성대학교 학생들을 위하여 교내 정보, 공지 등을 쉽게 얻을 수 있다.
 - 자주 사용하는 마을버스, 미세먼지 등의 정보 제공
 
 # <개발내용>
 - API Crawling을 사용하여 정보 추출
 - MS의 LUIS를 통한 자연어처리
 - MSSQL를 사용한 DB 구축, 관리
