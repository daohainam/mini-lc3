.ORIG x3000 

	LEA R0, hello
	PUTS
	
	LD R0,USER_CODE_ADDR
	JMP R0

USER_CODE_ADDR	.FILL x3000	
hello	.STRINGZ "Hello World!\n"	
	
.END   