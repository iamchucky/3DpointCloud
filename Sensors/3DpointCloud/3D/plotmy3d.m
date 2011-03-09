close all
clear all
clc

threeD = load('output.txt');

pose.x = threeD(:,1);
pose.y = threeD(:,2);
pose.z = threeD(:,3);
pose.yaw = threeD(:,4);
pose.pitch = threeD(:,5);
pose.roll = threeD(:,6);
K = [-686.869 0 297.687
     0 687.997 218.456
     0 0 1];
px = threeD(:,7);
py = threeD(:,8);
nx = threeD(:,9);
ny = threeD(:,10);
prevP = threeD(:,11:22);
nextP = threeD(:,23:34);
A = threeD(:,35:end-3);

prevPM = [prevP(1,1:4); prevP(1,5:8); prevP(1,9:12)];
nextPM = [nextP(1,1:4); nextP(1,5:8); nextP(1,9:12)];
AM = [A(1,1:4); A(1,5:8); A(1,9:12); A(1,13:end)];
[V,D] = eig(AM)
x0 = V(:,4)
AM*x0
x0./x0(4)

x = threeD(:,end-2);
y = threeD(:,end-1);
z = threeD(:,end);

a = -pi/2;
b = 0;
c = pi/2;
Trc = [0 0 0.5]';
Rrc = [cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c)
    sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c)
    -sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c)					 ];
RTrc = [Rrc Trc];

figure 
hold on
xlabel('x')
ylabel('y')
zlabel('z')
for n = 1:size(threeD,1)
%     T = [pose.x(n) pose.y(n) pose.z(n)]';
%     a = pose.yaw(n);
%     b = pose.pitch(n);
%     c = pose.roll(n);
%     R = [cos(a)*cos(b), cos(a)*sin(b)*sin(c)-sin(a)*cos(c), cos(a)*sin(b)*cos(c)+sin(a)*sin(c)
%     sin(a)*cos(b), sin(a)*sin(b)*sin(c)+cos(a)*cos(c), sin(a)*sin(b)*cos(c)-cos(a)*sin(c)
%     -sin(b),	   cos(b)*sin(c),					   cos(b)*cos(c)					 ];
%     RTwr = [R T];
%     M = [RTwr; zeros(1,3) 1];
%     
%     RT = [R*Rrc Trc+T; zeros(1,3) 1];
%     drawcam(RT,0.1,'-g');
%     drawbot(M,0.2,'-g');
plot3(threeD(n,end-2), threeD(n,end-1), threeD(n,end),'*');
pause(0.001);
end

% pose.x = 2.72296555;
% pose.y = -1.06462867914998;
% pose.z = 0.226634735864988;
% a = 3.03064168349532;
% b = 0.0110733457646236;
% c = 0.0262664426655966;

% RTinv = [Rrc'*R' Rrc'*(-R'*T-Trc); zeros(1,3) 1];
% 
% % P = K*R*[eye(3) T];
% P = K*RTinv(1:3,:);
% % P =
% %   1.0e+003 *
% %    -0.2982   -0.6396    0.2497   -2.7819
% %    -0.2199    0.2508    0.6402    0.6566
% %    -0.0010   -0.0000   -0.0000   -0.0006
% twoD = P*[zeros(3,1); 1];
% twoD = twoD./twoD(3)

% % hold on
% M = [RTwr; zeros(1,3) 1];
% % M = eye(4);
% drawcam(RTinv,0.1,'-g');
% hold on
% drawbot(M,0.2,'-g');
% xlabel('x')
% ylabel('y')
% zlabel('z')
% xmin = -5;
% xmax = 5;
% ymin = -3;
% ymax = 3;
% zmin = -1;
% zmax = 3;
% cmin = 1;
% cmax = 192;
% axis([xmin xmax ymin ymax zmin zmax cmin cmax])
% grid on