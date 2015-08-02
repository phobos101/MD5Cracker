# MD5Cracker
Parallel computing project to crack an MD5 hash.

This was part of my university degree. I created a MD5 cracker that has a server program and a cvlient program. The server sends out chunks of numbers to clients to process, if there is a match with hash the client sends the result to the server which then informs all the clients. If there is no match the client requests a new chunk of numbers to test. Naturally, the more clients connected the faster the result. 

Please see my algorithm diagrams for detailed info.

Client diagram - https://drive.google.com/file/d/0B3WmS-5LE2ANUC1iTjlsRmlxbGs/view?usp=sharing

Server diagram - https://drive.google.com/file/d/0B3WmS-5LE2ANQ1RYTEtIdFo4RTQ/view?usp=sharing
