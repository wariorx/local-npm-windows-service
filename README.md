# local-npm-windows-service

C# code for creating a Windows Service with local-npm

# Dependencies:
  local-npm
  
  nodejs

# Usage
  Open either windows shell and run:
  
    sc.exe create <new_service_name> binPath= "<path_to_the_service_executable>"
    
  Then just Start the service through either a shell or services.msc
